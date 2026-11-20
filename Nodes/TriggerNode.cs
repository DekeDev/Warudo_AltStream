using System.Collections.Generic;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;

namespace DEKE.AltStream {
    public abstract class TriggerNode : Node {
        protected abstract EventType EventType { get; set; }
        protected StreamData CurrentData { get; set; }
        protected AltStream MyPlugin;
        
        [FlowOutput] 
        public Continuation OnTrigger;
        
        [DataOutput] [Label("Event Data")]
        public StreamData GetEventData() => CurrentData;
        
        protected override void OnCreate() {
            base.OnCreate();
            MyPlugin = (AltStream)Plugin;
            MyPlugin.EventReceived += Plugin_OnEventReceived;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            MyPlugin.EventReceived -= Plugin_OnEventReceived;
        }

        protected virtual void Plugin_OnEventReceived(EventType eventType, StreamData streamData) {
            CurrentData = streamData;
        }
    }
}