using System;

namespace Network.WebRTC.Models
{
    [Serializable]
    public class SdpMessage
    {
        public string type; 
        public SdpPayload payload;
    }

    [Serializable]
    public class SdpPayload
    {
        public string sdp;
    }
}