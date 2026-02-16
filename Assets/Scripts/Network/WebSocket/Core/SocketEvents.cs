namespace Network.WebSocket.Core
{
    /// <summary>
    /// Constants for WebSocket event names.
    /// Used for communication protocol between Unity client and backend server.
    /// </summary>
    public static class SocketEvents
    {
        // Authentication events
        public const string AUTHENTICATE = "auth:login";
        public const string AUTH_SUCCESS = "auth:login_success";  
        public const string AUTH_ERROR = "auth:login_error";   
        
        // WebRTC signaling events (SDP exchange and ICE candidates)
        public const string WEBRTC_OFFER = "webrtc:offer";
        public const string WEBRTC_ANSWER = "webrtc:answer";
        public const string WEBRTC_ICE_CANDIDATE = "webrtc:ice-candidate";

        // Peer connection notification events
        public const string FRONTEND_CONNECTED = "frontend:connected";
        public const string ROBOT_CONNECTED = "robot:connected";
        public const string SERVER_SHUTDOWN = "server:shutdown";
        
        // Emergency event (not used in current implementation)
        public const string EMERGENCY_STOP = "emergency:stop";
        
        // General connection events
        public const string ERROR = "error";
        public const string HEARTBEAT = "heartbeat";
        public const string PONG = "pong";
        public const string DISCONNECT = "disconnect";
    }
}