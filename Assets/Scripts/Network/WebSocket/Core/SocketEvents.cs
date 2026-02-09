namespace Network.WebSocket.Core
{
    public static class SocketEvents
    {
        // Authentication events
        public const string AUTHENTICATE = "auth:login";
        public const string AUTH_SUCCESS = "auth:success";
        public const string AUTH_ERROR = "auth:error";
        
        // WebRTC signaling events
        public const string WEBRTC_OFFER = "webrtc:offer";
        public const string WEBRTC_ANSWER = "webrtc:answer";
        public const string WEBRTC_ICE_CANDIDATE = "webrtc:ice-candidate";


        // Notification events
        public const string FRONTEND_CONNECTED = "frontend:connected";
        public const string ROBOT_CONNECTED = "robot:connected";
        public const string SERVER_SHUTDOWN = "server:shutdown";
        
        // Emergency evetns
        public const string EMERGENCY_STOP = "emergency:stop";
        
        // General events
        public const string ERROR = "error";
        public const string HEARTBEAT = "heartbeat";
        public const string PONG = "pong";
        public const string DISCONNECT = "disconnect";
    }
}