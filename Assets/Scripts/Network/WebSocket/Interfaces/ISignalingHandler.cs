using System;
using Network.WebSocket.Models;

namespace Network.WebSocket.Interfaces
{
    public interface ISignalingHandler
    {
        event Action<WebRTCOffer> OnOfferReceived;
        event Action<WebRTCAnswer> OnAnswerReceived;
        event Action<ICECandidate> OnICECandidateReceived;
        event Action<string> OnRobotConnected;
        event Action<string> OnFrontendConnected;
        
        void SendOffer(string sdp, string target = null);
        void SendAnswer(string sdp, string target = null);
        void SendICECandidate(string candidate, string sdpMid, int sdpMLineIndex, string target = null);
        void Dispose();
    }
}