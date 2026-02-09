using System;

namespace Network.WebRTC.Models
{
    [Serializable]
    public class IceCandidateMessage
    {
        public string type;
        public IceCandidatePayload payload;
    }

    [Serializable]
    public class IceCandidatePayload
    {
        public string candidate;
        public string sdpMid;
        public int sdpMLineIndex;
    }
}