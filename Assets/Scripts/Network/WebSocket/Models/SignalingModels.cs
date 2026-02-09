using System;

namespace Network.WebSocket.Models
{
    /// <summary>
    /// WebRTC Offer
    /// </summary>
    [Serializable]
    public class WebRTCOffer
    {
        public string sdp;
        public string target; 
        public string from;   
    }

    /// <summary>
    /// WebRTC Answer
    /// </summary>
    [Serializable]
    public class WebRTCAnswer
    {
        public string sdp;
        public string target;
        public string from;
    }

    /// <summary>
    /// ICE Candidate
    /// </summary>
    [Serializable]
    public class ICECandidate
    {
        public string candidate;
        public string sdpMid;
        public int sdpMLineIndex;
        public string target;
        public string from;
    }

    /// <summary>
    /// Notification de connexion
    /// </summary>
    [Serializable]
    public class PeerConnectionNotification
    {
        public string username;
        public string role;
        public long timestamp;
    }
}