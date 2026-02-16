using System;

namespace Network.WebRTC.Models
{
    /// <summary>
    /// Models for WebRTC signaling messages (SDP and ICE candidates).
    /// Used for establishing peer-to-peer connections.
    /// </summary>

    /// <summary>
    /// Wrapper for ICE candidate messages sent via signaling.
    /// </summary>
    [Serializable]
    public class IceCandidateMessage
    {
        public string type;                     // Message type, e.g., "ice-candidate"
        public IceCandidatePayload payload;     // ICE candidate details
    }

    /// <summary>
    /// Contains ICE candidate connection information.
    /// ICE = Interactive Connectivity Establishment (NAT traversal).
    /// </summary>
    [Serializable]
    public class IceCandidatePayload
    {
        public string candidate;                // The ICE candidate string
        public string sdpMid;                   // Media stream ID
        public int sdpMLineIndex;               // Media line index in SDP
    }

    /// <summary>
    /// Wrapper for SDP (Session Description Protocol) messages.
    /// Contains offer or answer for WebRTC negotiation.
    /// </summary>
    [Serializable]
    public class SdpMessage
    {
        public string type;                     // "offer" or "answer"
        public SdpPayload payload;              // SDP content
    }

    /// <summary>
    /// Contains the actual SDP content describing media capabilities.
    /// </summary>
    [Serializable]
    public class SdpPayload
    {
        public string sdp;                      // The SDP string
    }
}