using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DEKE.AltStream {
    /// <summary>
    /// Helper class to create a one-shot request to the AltStream API. Returns an <see cref="IDataContainer"/>
    /// of the given type or null if the request failed.
    /// </summary>
    /// <typeparam name="T">Any<see cref="IDataContainer"/></typeparam>
    public class ApiRequest<T> where T : IDataContainer {
        public event Action<T> RequestCompleted; 
        
        private UnityWebRequest WebRequest { get; set; }
        private T m_container;
        private readonly string m_url;
        private readonly string m_token;
        
        public ApiRequest(string url, string token, Action<T> onRequestCompletedCallback) {
            m_url = url;
            m_token = token;
            RequestCompleted = onRequestCompletedCallback;
            
            StartRequest();
        }

        private void StartRequest() {
            string requestUrl = m_token == string.Empty ? m_url : $"{m_url}?token={m_token}";

            if (m_url == string.Empty) {
                Debug.LogError("[DEKE.AltStream.ApiRequest] URL has not been set for API request!");
                return;
            }
            
            Debug.Log($"[DEKE.AltStream.ApiRequest] Creating UnityWebRequest: {requestUrl}");
            WebRequest = UnityWebRequest.Get(requestUrl);
            WebRequest.SendWebRequest().completed += WebRequestOnCompleted;
        }

        private void WebRequestOnCompleted(AsyncOperation res) {
            if (WebRequest.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"[DEKE.AltStream.ApiRequest] WebRequest Error: {WebRequest.error}");
                RequestCompleted?.Invoke(m_container);
                return;
            }
            
            string json = WebRequest.downloadHandler.text;
            Debug.Log($"[DEKE.AltStream.ApiRequest] Received data: {json}");
            m_container = JsonConvert.DeserializeObject<T>(json);
            
            RequestCompleted?.Invoke(m_container);
        }
    }
}