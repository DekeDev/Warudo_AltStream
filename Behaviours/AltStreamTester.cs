using UnityEngine;

namespace DEKE.AltStream {
    /// <summary>
    /// MonoBehaviour that's basically a copy of <see cref="AltStream"/> to allow for testing plugin features inside the Unity
    /// Editor. Should not be used outside the editor.
    /// </summary>
    public class AltStreamTester : MonoBehaviour {
#if UNITY_EDITOR
        [SerializeField] private string userName;
        [SerializeField] private string botToken;
        
        private SSE m_sse;
        private SSEStatus m_sseStatus;

        private void Start() {
            if (userName != string.Empty && botToken != string.Empty) {
                Debug.Log("[DEKE.AltStream.AltStreamTester] Bot Token already exists, trying to connect...");
                ConnectToAltStream();
            }

            string url = Endpoints.Rewards.Replace("[channel_name]", userName);
            ApiRequest<RewardsData> request = new(url, botToken, OnRewardDataReceived);
        }
        
        private void Update() {
            if (m_sse == null) {
                m_sseStatus = SSEStatus.Disconnected;
                return;
            }
            
            if (m_sseStatus != m_sse.Status) {
                Debug.Log("[DEKE.AltStream.AltStreamTester] Connection Status changed");
                m_sseStatus = m_sse.Status;
            }
        }
        
        private void ConnectToAltStream() {
            if (m_sseStatus == SSEStatus.Connected || m_sseStatus == SSEStatus.Connecting) return;
            if (userName != string.Empty && botToken != string.Empty) {
                if (m_sse != null) {
                    Debug.Log("[DEKE.AltStream.AltStreamTester] SSE Handler Already exists, aborting...");
                    return;
                }

                Debug.Log("[DEKE.AltStream.AltStreamTester] Creating new SSE Handler");
                m_sse = new SSE(userName, botToken);
            }
        }

        private void OnRewardDataReceived(RewardsData data) {
            if (data == null) {
                Debug.LogError("[DEKE.AltStream.AltStreamTester] Received empty data!");
                return;
            }
            
            Debug.Log(data.data.rewards[0].name);
        }
#endif
    }
}