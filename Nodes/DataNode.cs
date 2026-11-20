using Warudo.Core.Graphs;

namespace DEKE.AltStream {
    public abstract class DataNode : Node {
        protected AltStream MyPlugin;
        
        protected override void OnCreate() {
            base.OnCreate();
            MyPlugin = (AltStream)Plugin;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
        }
    }
}