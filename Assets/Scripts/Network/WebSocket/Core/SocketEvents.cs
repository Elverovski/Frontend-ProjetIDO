namespace Network.WebSocket.Core
{
    /// <summary>
    /// Constantes pour les événements WebSocket
    /// </summary>
    public static class SocketEvents
    {
        // AUTHENTIFICATION
        public const string AUTHENTICATE = "authenticate";
        public const string AUTH_SUCCESS = "auth:success";
        public const string AUTH_ERROR = "auth:error";
        
        // WEBRTC SIGNALING
        public const string WEBRTC_OFFER = "webrtc:offer";
        public const string WEBRTC_ANSWER = "webrtc:answer";
        public const string WEBRTC_ICE_CANDIDATE = "webrtc:ice-candidate";


        // NOTIFICATIONS
        public const string FRONTEND_CONNECTED = "frontend:connected";
        public const string ROBOT_CONNECTED = "robot:connected";
        public const string SERVER_SHUTDOWN = "server:shutdown";
        
        // EMERGENCY FALLBACK
        public const string EMERGENCY_STOP = "emergency:stop";
        
        // GENERAL
        public const string ERROR = "error";
        public const string HEARTBEAT = "heartbeat";
        public const string PONG = "pong";
        public const string DISCONNECT = "disconnect";
    }
}