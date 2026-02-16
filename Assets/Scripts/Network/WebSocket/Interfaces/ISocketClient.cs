using System;

namespace Network.WebSocket.Interfaces
{
    public interface ISocketClient
    {
        bool IsConnected { get; }
        
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<string, string> OnMessage;
        
        void Connect(string url);
        void SendMessage(string eventName, string data);
        void Disconnect();
    }
}