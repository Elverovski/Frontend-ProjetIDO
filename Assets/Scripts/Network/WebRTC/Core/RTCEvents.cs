namespace Network.WebRTC.Core
{
    public static class RTCEvents
    {
        // Peer connection state events
        public const string PEER_CONNECTING = "peer:connecting";
        public const string PEER_CONNECTED = "peer:connected";
        public const string PEER_DISCONNECTED = "peer:disconnected";
        public const string PEER_FAILED = "peer:failed";
        
        // Data channel events
        public const string DATA_CHANNEL_OPEN = "datachannel:open";
        public const string DATA_CHANNEL_CLOSED = "datachannel:closed";
        public const string DATA_CHANNEL_ERROR = "datachannel:error";
        
        // Video stream events
        public const string VIDEO_TRACK_ADDED = "video:track_added";
        public const string VIDEO_TRACK_REMOVED = "video:track_removed";
        
        // Stats update events
        public const string STATS_UPDATED = "stats:updated";
    }
}