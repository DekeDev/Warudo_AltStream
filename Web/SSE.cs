using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DEKE.AltStream {
    /// <summary>
    /// Main class that handles the connection to the SSE stream and event parsing. This thing is a little jank and uses
    /// some workarounds, since Unity isn't really built well to support EventSource, so we go via a normal UnityWebRequest
    /// that is basically always open.
    /// </summary>
    public class SSE {
        public event Action<SSEStatus> ConnectionStatusChanged;
        public event Action<EventType, StreamData> EventReceived;
        
        private readonly string m_user;
        private readonly string m_token;
        private SSEStatus m_status;

        /// <summary>
        /// The current status of the API connection, will automatically invoke an event
        /// when the status changes.
        /// </summary>
        public SSEStatus Status {
            get => m_status;
            private set {
                if (m_status != value) {
                    ConnectionStatusChanged?.Invoke(value);
                }

                m_status = value;
            }
        }

        private UnityWebRequest WebRequest { get; set; }
        
        public SSE(string user, string token) {
            m_user = user;
            m_token = token;
        }
        
        /// <summary>
        /// Sets up an event-stream UnityWebRequest that will download data for as long as the
        /// connection to the API is open.
        /// </summary>
        public void ConnectToStream() { 
            if (!string.IsNullOrEmpty(m_token)) { 
                string requestUrl = $"{Endpoints.Stream}{m_user}&token={m_token}";

                m_status = SSEStatus.Connecting;
                
                Debug.Log($"[DEKE.AltStream.SSEConnector] Creating UnityWebRequest: {requestUrl}");
                WebRequest = new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbGET);
                WebRequest.SetRequestHeader("Content-Type", "text/event-stream");
                WebRequest.SetRequestHeader("Cache-Control", "no-cache");

                SseDownloadHandler handler = new(SseDownloadHandler_OnMessageReceived, SseDownloadHandler_OnStreamClosed, SseDownloadHandler_OnConnectionEstablished);
                WebRequest.downloadHandler = handler;

                Debug.Log("[DEKE.AltStream.SSEConnector] Starting UnityWebRequest...");
                WebRequest.SendWebRequest().completed += WebRequestOnCompleted;
            } else {
                Debug.LogError("[DEKE.AltStream.SSEConnector] Invalid bot token! Cannot connect to altstream without a valid token for this channel.");
            }
        }
        
        /// <summary>
        /// Event method that gets called whenever the WebRequest's DownloadHandler receives
        /// a new message from the event stream
        /// </summary>
        /// <param name="data"></param>
        private void SseDownloadHandler_OnMessageReceived(string data) {
            StreamData streamData = JsonConvert.DeserializeObject<StreamData>(data);
            EventType eventType = EventType.Null;
            
            if (streamData.type != string.Empty) {
                if (streamData.type == "redemption") {
                    eventType = EventType.RewardRedeemed;
                    Debug.Log($"[DEKE.AltStream.SSEConnector] Event is a Reward Redemption. Reward Name: {streamData.redemption.rewardName}");
                }
                EventReceived?.Invoke(eventType, streamData);
            }
        }
        
        /// <summary>
        /// Event method that gets called when the WebRequest's DownloadHandler finishes the
        /// data stream (at which point the connection is considered closed)
        /// </summary>
        private void SseDownloadHandler_OnStreamClosed() {
            Debug.Log("[DEKE.AltStream.SSEConnector] Connection to AltStream lost, reconnecting...");
            DisposeWebRequest(true);
        }
    
        /// <summary>
        /// Event method that gets called when the WebRequest's DownloadHandler receives the
        /// first "ping" message in a newly established event stream
        /// </summary>
        private void SseDownloadHandler_OnConnectionEstablished() {
            Debug.Log("[DEKE.AltStream.SSEConnector] Connection to Altstream established. Listening...");
            Status = SSEStatus.Connected;
        }
        
        /// <summary>
        /// Will be called by the AsyncOperation from establishing the UnityWebRequest. Due to the fact
        /// it connects to a realtime data stream, this *should* only ever get called when the connection
        /// fails or is aborted.
        /// </summary>
        /// <param name="res"></param>
        private void WebRequestOnCompleted(AsyncOperation res) {
            if (WebRequest == null) {
                Status = SSEStatus.Disconnected;
                return;
            }
            
            if (WebRequest.result != UnityWebRequest.Result.Success) {
                Status = SSEStatus.Error;
                Debug.LogError($"[DEKE.AltStream.SSEConnector] Couldn't connect to Altstream SSE stream: {WebRequest.error}");
                DisposeWebRequest(true);
            }
        }
        
        /// <summary>
        /// Aborts/Disposes the currently active UnityWebRequest (if applicable).
        /// Can optionally try to automatically reconnect to the service.
        /// </summary>
        /// <param name="autoReconnect"></param>
        public void DisposeWebRequest(bool autoReconnect) {
            Status = SSEStatus.Disconnected;
            Debug.Log("[DEKE.AltStream.SSEConnector] Trying to dispose WebRequest.");
        
            if (WebRequest != null) {
                Debug.Log("[DEKE.AltStream.SSEConnector] WebRequest exists, aborting/disposing.");
                WebRequest.Abort();
                WebRequest.Dispose();
                WebRequest = null;
            }

            if (autoReconnect) {
                Debug.Log("[DEKE.AltStream.SSEConnector] Attempting to automatically reconnect...");
                    ConnectToStream();
            }
        }
        
        /// <summary>
        /// Custom download handler for SSE events for use with UnityWebRequests.
        /// </summary>
        private class SseDownloadHandler : DownloadHandlerScript {
            private event Action<string> DataReceived;
            private event Action StreamClosed;
            private event Action ConnectionEstablished;
            private readonly StringBuilder m_buffer = new();

            private bool m_connectionEstablished;
            
            public SseDownloadHandler(Action<string> onDataReceivedCallback, Action onCloseCallback, Action onConnectionCallback) : base(new byte[1024]) {
                DataReceived = onDataReceivedCallback;
                StreamClosed = onCloseCallback;
                ConnectionEstablished = onConnectionCallback;
            }

            /// <summary>
            /// Override of the Unity DownloadHandler for handling any received data. We read the received buffer into a string and then split
            /// this string via the double newlines the data *should* be sent with from the server. We then parse every piece of data separately.
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="dataLength"></param>
            /// <returns></returns>
            protected override bool ReceiveData(byte[] buffer, int dataLength) {
                if (buffer == null || dataLength == 0) return false;
                
                string chunk = Encoding.UTF8.GetString(buffer, 0, dataLength);
                m_buffer.Append(chunk);

                string currentEvent = m_buffer.ToString();
                int splitIndex;

                while ((splitIndex = currentEvent.IndexOf("\n\n", StringComparison.Ordinal)) >= 0) {
                    string rawEvent = currentEvent.Substring(0, splitIndex);
                    currentEvent = currentEvent.Substring(splitIndex + 2);
                    ParseEvent(rawEvent);
                }

                m_buffer.Clear();
                m_buffer.Append(currentEvent);

                return true;
            }

            /// <summary>
            /// In the case of an SSE stream, CompleteContent() should only be called when the connection is lost, so we can use it as
            /// an event to re-establish API connection.
            /// TODO: This hasn't been properly tested yet, since I don't have a great way to simulate a loss of connection right now.
            /// </summary>
            protected override void CompleteContent() {
                StreamClosed?.Invoke();
            }

            /// <summary>
            /// Parses the currently received event. SSE events split their data with newlines, so we can easily create an array of strings
            /// that we can parse line by line.
            /// </summary>
            /// <param name="raw"></param>
            private void ParseEvent(string raw) {
                string[] lines = raw.Split('\n');
                foreach (string line in lines) {
                    // We look for the first instance of a "ping" message to make sure we established a connection to the API.
                    // UnityWebRequest will always be "in progress" when connecting to a live event stream, so this is kind of
                    // a workaround to see if we actually connected to the SSE stream.
                    if (line.StartsWith(": ping") && !m_connectionEstablished) {
                        m_connectionEstablished = true;
                        ConnectionEstablished?.Invoke();
                    }
                    
                    // If the current line is not a data payload, we're not interested
                    if (!line.StartsWith("data:")) continue;
                    
                    string payload = line.Substring(5).TrimStart();
                    DataReceived?.Invoke(payload);
                }
            }
        }
    }
}