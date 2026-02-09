using System;

namespace Network.WebRTC.Models
{
    /// <summary>
    /// Represents a message containing an ICE candidate
    /// </summary>
    [Serializable]
    public class IceCandidateMessage
    {
        public string type;          // Message type, e.g., "ice-candidate"
        public IceCandidatePayload payload; // ICE candidate details
    }

    // ICE candidate details
    [Serializable]
    public class IceCandidatePayload
    {
        public string candidate;     // The ICE candidate string
        public string sdpMid;        // SDP media ID
        public int sdpMLineIndex;    // SDP media line index
    }
}