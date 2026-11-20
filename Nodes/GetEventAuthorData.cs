using System;
using Warudo.Core.Attributes;

namespace DEKE.AltStream {
    [NodeType(Id = "4201f526-5f3c-4460-8a7a-8e1abddcbefa", Title = "Get AltStream Event Author Data", Category = "AltStream Integration")]
    public class GetEventAuthorData : DataNode {
        [DataInput] [Description("Reward to retrieve data for")]
        public StreamData Data;

        [DataOutput] [Label("ID")] public string GetId() => !HasValidData() ? string.Empty : Data.author.id;
        [DataOutput] [Label("Display name")] public string GetDisplayName() => !HasValidData() ? string.Empty : Data.author.displayName;
        [DataOutput] [Label("Channel name")] public string GetChannelName() => !HasValidData() ? string.Empty : Data.author.channelName;
        [DataOutput] [Label("Pronouns")] public string GetPronouns() => !HasValidData() ? string.Empty : Data.author.pronouns;
        [DataOutput] [Label("Color")] public string GetColor() => !HasValidData() ? string.Empty : Data.author.color;
        [DataOutput] [Label("Badges array")] public string[] GetBadges() => !HasValidData() ? Array.Empty<string>() : Data.author.badges;
        
        private bool HasValidData() {
            return (Data != null);
        }
    }
}