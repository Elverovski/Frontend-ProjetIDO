using System;

namespace Network.WebSocket.Models
{
    // WebRTC offer data
    [Serializable]
    public class WebRTCOffer
    {
        public string sdp;
        public string target; 
        public string from;   
    }

    // WebRTC answer data
    [Serializable]
    public class WebRTCAnswer
    {
        public string sdp;
        public string target;
        public string from;
    }

    // ICE candidate data
    [Serializable]
    public class ICECandidate
    {
        public string candidate;
        public string sdpMid;
        public int sdpMLineIndex;
        public string target;
        public string from;
    }

    // Peer connection notification
    [Serializable]
    public class PeerConnectionNotification
    {
        public string username;
        public string role;
        public long timestamp;
    }
}