using System;
using Network.WebSocket.Models;

namespace Network.WebSocket.Interfaces
{
    public interface IWebSocketManager
    {
        IAuthHandler Auth { get; }
        ISignalingHandler Signaling { get; }
        
        bool IsConnected { get; }
        bool IsAuthenticated { get; }
        string Username { get; }
        
        event Action OnConnected;
        event Action OnDisconnected;
        event Action OnAuthenticated;
        event Action<string> OnError;
        
        void Connect();
        void Disconnect();
        void Login(string username, string password);
    }
}