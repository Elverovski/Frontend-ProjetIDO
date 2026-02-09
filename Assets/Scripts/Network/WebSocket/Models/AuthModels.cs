using System;

namespace Network.WebSocket.Models
{
    // Login request data
    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
        public string deviceId;
    }

    // Login response data
    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public string token;
        public string userId;
        public string message;
        public UserData userData;
    }

    // User information
    [Serializable]
    public class UserData
    {
        public string username;
        public string role;
        public string[] permissions;
    }

    // Generic error response
    [Serializable]
    public class ErrorResponse
    {
        public string message;
        public string code;
    }
}