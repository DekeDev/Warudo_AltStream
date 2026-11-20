using System;
using System.Collections.Generic;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Plugins;
using Warudo.Core.Scenes;
using Warudo.Core.Serializations;
using Warudo.Plugins.Core.Mixins;

namespace DEKE.AltStream {
    /// <summary>
    /// Main Plugin that handles setup, connection to the AltStream API and handling of any events coming in.
    /// </summary>
    [PluginType(Id = "DEKE.AltStream", Name = "AltStream Integration", Description = Info, Author = "DEKE", Version = "0.2.0",
        NodeTypes = new [] {
            typeof(RewardRedeemedNode),
            typeof(GetRewardDataNode),
            typeof(GetEventAuthorData),
        },
        Icon = SVG.Logo)]
    public class AltStream : Plugin {
        public event Action<EventType, StreamData> EventReceived;
        public event Action SceneLoaded;
        
        private SSE m_sse;
        private SSEStatus m_sseStatus;

        /// <summary>
        /// Cache for relevant and helpful data from the API. Mostly used for QoL or to read out event
        /// data in Warudo's blueprints
        /// </summary>
        public Cache Cache { get; private set; }
        
#region Warudo flow properties
        [Mixin] public ToolbarItemMixin ToolbarItem;
        
        [Markdown(primary: true)] public const string Info = "Plugin with useful nodes to interact with the AltStream API";
        [Markdown] public string PluginHelp = "Make sure to use the correct capitalization for your user name and be careful about not showing your bot token on stream!";
        
        [DataInput] public string UserName;
        [DataInput] public string BotToken;
        
        [Markdown] public string TokenHelp = string.Empty;
        
        [Trigger] public void CreateBotToken() => OpenDashboardBotSettings();
        
        [Markdown] public string ConnectionStatus = "Disconnected from AltStream Service";
        
        [Trigger] public void ConnectToAltstream() => DoConnect();
        [Trigger] public void Disconnect() => DoDisconnect();
        [Trigger] public void RebuildCache() => DoRebuild();
#endregion
        
        protected override void OnCreate() {
            base.OnCreate();
            ToolbarItem.SetIcon(SVG.Logo);
            ToolbarItem.OnTrigger = () => Context.Service.NavigateToPlugin(Type.Id);
            Watch<string>(nameof(UserName), UpdateTokenHelpText);
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            Debug.Log("[DEKE.AltStream] Plugin destroyed. Disposing SSE Handler");
            m_sse?.DisposeWebRequest(false);
        }

        private void UpdateTokenHelpText(string from = "", string to = "") {
            string user = UserName == string.Empty ? "[your_username]" : UserName;
            
            TokenHelp = $"You can create a new bot token at https://altstream.online/{user}/settings?tab=bot";
            BroadcastDataInput(nameof(TokenHelp));
        }

        private void DoConnect() {
            if (m_sseStatus == SSEStatus.Connected || m_sseStatus == SSEStatus.Connecting) return;
            if (UserName != string.Empty && BotToken != string.Empty) {
                if (m_sse != null) {
                    Debug.Log("[DEKE.AltStream] SSE Handler Already exists, aborting...");
                    return;
                }
                
                Debug.Log("[DEKE.AltStream] Creating new SSE Handler");
                m_sse = new SSE(UserName, BotToken);
                m_sse.ConnectionStatusChanged += SSE_OnConnectionStatusChanged;
                m_sse.EventReceived += SSE_OnEventReceived;
                m_sse.ConnectToStream();
                
                // Cache certain data from the AltStream API
                Cache = new Cache(UserName, BotToken);
            }
        }

        private void DoDisconnect() {
            if (m_sse == null) {
                Debug.Log("[DEKE.AltStream] Cannot disconnect, no SSE Handler exists.");
                return;
            }
            
            m_sse.DisposeWebRequest(false);
            m_sse = null;
        }

        private void DoRebuild() {
            Cache = new Cache(UserName, BotToken);
        }

        private void OpenDashboardBotSettings() {
            if (UserName != string.Empty) {
                Application.OpenURL($"https://altstream.online/{UserName}/settings?tab=bot");
            }
        }
        
        /// <summary>
        /// OnSceneLoaded is used to set up the AltStream connection if user credentials have already been set up.
        /// We use this instead of OnCreate() to make sure Warudo has loaded all necessary data by the time we need it.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="serializedScene"></param>
        public override void OnSceneLoaded(Scene scene, SerializedScene serializedScene) {
            SceneLoaded?.Invoke();
            
            UpdateTokenHelpText();
            
            if (UserName != string.Empty && BotToken != string.Empty) {
                Debug.Log("[DEKE.AltStream] Bot Token already exists, trying to connect...");
                DoConnect();
            }
        }

        private void SSE_OnConnectionStatusChanged(SSEStatus status) {
            m_sseStatus = status;
            ConnectionStatus = Strings.SSEStatusStrings[m_sseStatus];
            BroadcastDataInput(nameof(ConnectionStatus));
            
            ToolbarItem.SetEnabled(m_sseStatus == SSEStatus.Connected);
            ToolbarItem.SetTooltip(m_sseStatus == SSEStatus.Connected ? "Connected to AltStream!" : "Disconnected.");
        }
        
        private void SSE_OnEventReceived(EventType eventType, StreamData streamData) {
            EventReceived?.Invoke(eventType, streamData);
        }
    }
}