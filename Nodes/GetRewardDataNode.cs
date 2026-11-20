using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data;

namespace DEKE.AltStream {
    [NodeType(Id = "b0bec425-4b63-4226-92b3-24252f6ddb2b", Title = "Get AltStream Reward Data", Category = "AltStream Integration")]
    public class GetRewardDataNode : DataNode {
        private List<RewardsData.Reward> m_rewards;
        private RewardsData.Reward m_currentReward = new();
        
        [DataInput] [Description("Reward to retrieve data for")] [AutoComplete(nameof(AutoCompleteRewardName), forceSelection: true)]
        public string RewardName;

        [DataOutput] [Label("Description")]
        public string GetDescription() => !HasValidReward() ? string.Empty : m_currentReward.description;
        
        [DataOutput] [Label("Cost")]
        public int GetCost() => !HasValidReward() ? -1 : m_currentReward.cost;
        
        [DataOutput] [Label("Is enabled?")]
        public bool GetEnabled() => HasValidReward() && m_currentReward.enabled;
        
        [DataOutput] [Label("Is highlight?")]
        public bool GetHighlight() => HasValidReward() && m_currentReward.isHighlight;
        
        [DataOutput] [Label("Cooldown (s)")]
        public float? GetCooldown() => !HasValidReward() ? null : m_currentReward.CooldownSeconds;
        
        [DataOutput] [Label("Global cooldown (s)")]
        public float? GetGlobalCooldown() => !HasValidReward() ? null : m_currentReward.GlobalCooldownSeconds;
        
        [DataOutput] [Label("Max per stream")]
        public int? GetMaxPerStream() => !HasValidReward() ? null : m_currentReward.MaxPerStream;
        
        [DataOutput] [Label("Max global per stream")]
        public int? GetMaxGlobalPerStream() => !HasValidReward() ? null : m_currentReward.MaxGlobalPerStream;
        
        protected override void OnCreate() {
            base.OnCreate();
            m_rewards = MyPlugin.Cache.RewardList;
            Watch<string>(nameof(RewardName), (from, to) => {
                m_currentReward = m_rewards.Find(reward => reward.name == to);
            });
        }

        protected async UniTask<AutoCompleteList> AutoCompleteRewardName() {
            List<RewardsData.Reward> rewards = MyPlugin.Cache.RewardList;

            return AutoCompleteList.Single(rewards.Select(reward => new AutoCompleteEntry {
                label = reward.name,
                value = reward.name
            }).ToList());
        }

        private bool HasValidReward() {
            return (RewardName != string.Empty && m_currentReward.name != string.Empty);
        }
    }
}