using System.Collections.Generic;
using UnityEngine;

namespace DEKE.AltStream {
    /// <summary>
    /// Data container that will be kept in memory as long as the AltStream plugin is active. Holds any AltStream data that needs
    /// to be cached in order for certain functionality and quality-of-life options to work.
    /// </summary>
    public class Cache : IDataContainer {
        public List<RewardsData.Reward> RewardList = new();

        private string m_user;
        private string m_token;
        
        public Cache(string user, string token) {
            m_user = user;
            m_token = token;
            
            CacheRewards();
        }

        private void CacheRewards() {
            string url = Endpoints.Rewards.Replace("[channel_name]", m_user);
            ApiRequest<RewardsData> request = new(url, m_token, OnRewardDataReceived);
        }

        private void OnRewardDataReceived(RewardsData data) {
            RewardList = new List<RewardsData.Reward>();
            
            if (data == null) {
                Debug.LogError("[DEKE.AltStream.Cache] Received empty data!");
                return;
            }

            foreach (RewardsData.Reward reward in data.data.rewards) {
                RewardList.Add(reward);
            }
        }
    }
}