using System.Collections.Generic;

namespace DEKE.AltStream {
    public static class Strings {
        public static Dictionary<SSEStatus, string> SSEStatusStrings = new() {
            {SSEStatus.Disconnected, "Disconnected from AltStream"},
            {SSEStatus.Error, "Connected to AltStream!"},
            {SSEStatus.Connecting, "Connecting..."},
            {SSEStatus.Connected, "Error while connecting to AltStream!"},
        };
    }
}