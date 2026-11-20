using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data;

namespace DEKE.AltStream {
    [NodeType(Id = "d4af7e8c-16a2-406a-b7a7-1da628416766", Title = "On AltStream Reward Redeemed", Category = "AltStream Integration")]
    public class RewardRedeemedNode : TriggerNode {
        protected override EventType EventType { get; set; } = EventType.RewardRedeemed;
        
        [DataInput] [Description("Reward to check")] [AutoComplete(nameof(AutoCompleteRewardName), forceSelection: true)]
        public string RewardName;
        
        protected async UniTask<AutoCompleteList> AutoCompleteRewardName() {
            List<RewardsData.Reward> rewards = MyPlugin.Cache.RewardList;

            return AutoCompleteList.Single(rewards.Select(reward => new AutoCompleteEntry {
                label = reward.name,
                value = reward.name
            }).ToList());
        }
        
        protected override void Plugin_OnEventReceived(EventType eventType, StreamData streamData) {
            base.Plugin_OnEventReceived(eventType, streamData);
            if (EventType != eventType) {
                Debug.Log($"[DEKE.AltStream.RewardRedeemedNode] EventType ({eventType}) doesn't match!");
                return;
            }
            
            if (RewardName != streamData.redemption.rewardName) {
                Debug.Log($"[DEKE.AltStream.RewardRedeemedNode] RewardName ({streamData.redemption.rewardName}) doesn't match!");
                return;
            }
            
            Debug.Log($"[DEKE.AltStream.RewardRedeemedNode] Invoking flow...");
            InvokeFlow(nameof(OnTrigger));
        }
    }
}