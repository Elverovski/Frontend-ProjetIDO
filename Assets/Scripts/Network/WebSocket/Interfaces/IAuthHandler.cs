using System;
using Network.WebSocket.Models;

namespace Network.WebSocket.Interfaces
{
    public interface IAuthHandler
    {
        bool IsAuthenticated { get; }
        string Token { get; }
        string Username { get; }
        
        event Action<LoginResponse> OnLoginSuccess;
        event Action<string> OnLoginError;
        event Action OnLogout;
        
        void Login(string username, string password, string deviceId = null);
        void Logout();
        void Dispose();
    }
}