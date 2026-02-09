using System;

namespace Network.WebRTC.Models
{
    /// <summary>
    /// Represents a message containing an SDP (offer/answer)
    /// </summary>
    [Serializable]
    public class SdpMessage
    {
        public string type;       // Message type, e.g., "offer" or "answer"
        public SdpPayload payload; // SDP content
    }

    // SDP content details
    [Serializable]
    public class SdpPayload
    {
        public string sdp;        // The SDP string
    }
}