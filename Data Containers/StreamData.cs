using System;

namespace DEKE.AltStream {
    /// <summary>
    /// Container class to allow for clean deserialization of data from AltStream SSE events.
    /// Handles the endpoint: /api/v2/chat/stream
    /// </summary>
    [Serializable]
    public class StreamData : IDataContainer {
        public string type;
        public Author author;
        public Message message;
        public Redemption redemption;

        [Serializable]
        public class Author {
            public string id;
            public string displayName;
            public string channelName;
            public string pronouns;
            public string color;
            public Roles roles;
            public string[] badges;
            
            [Serializable]
            public class Roles {
                public bool broadcaster;
                public bool moderator;
                public bool channelSubscriber;
                public bool altstreamSubscriber;
                public bool follower;
            }
        }

        [Serializable]
        public class Message {
            public string id;
            public string text;
            public Sticker sticker;
            
            [Serializable]
            public class Sticker {
                public string url;
                public string name;
            }
        }
        
        [Serializable]
        public class Redemption {
            public string rewardName;
        }
    }
}