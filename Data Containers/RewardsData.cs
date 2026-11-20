using System;

namespace DEKE.AltStream {
    [Serializable]
    public class RewardsData : IDataContainer {
        public Data data;
        
        [Serializable]
        public class Data {
            public Reward[] rewards;
        }
        
        [Serializable]
        public class Reward {
            public int id;
            public string name;
            public string description;
            public int cost;
            public bool enabled;
            public bool isHighlight;
            public float? CooldownSeconds;
            public float? GlobalCooldownSeconds;
            public int? MaxPerStream;
            public int? MaxGlobalPerStream;
        }
    }
}