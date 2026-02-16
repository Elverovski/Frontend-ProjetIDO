using System;

namespace Network.WebRTC.Interfaces
{
    public interface IPeerConnectionHandler
    {
        event Action OnNegotiationComplete;
        event Action<string> OnNegotiationError;
        
        void InitiateConnection(string targetPeerUsername);
        void Dispose();
    }
}