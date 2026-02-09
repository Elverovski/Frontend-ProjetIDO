using System;

namespace Network.WebSocket.Models
{
    /// <summary>
    /// Requête de login
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
        public string deviceId;
    }

    /// <summary>
    /// Réponse de login réussie
    /// </summary>
    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public string token;
        public string userId;
        public string message;
        public UserData userData;
    }

    /// <summary>
    /// Données utilisateur
    /// </summary>
    [Serializable]
    public class UserData
    {
        public string username;
        public string role;
        public string[] permissions;
    }

    /// <summary>
    /// Erreur générique
    /// </summary>
    [Serializable]
    public class ErrorResponse
    {
        public string message;
        public string code;
    }
}