using System;
using Unity.WebRTC;

namespace Network.WebRTC.Core
{
    [Serializable]
    public class RTCConfig
    {
        // List of STUN servers for NAT traversal
        public string[] iceServers = new string[]
        {
            "stun:stun.l.google.com:19302",
            "stun:stun1.l.google.com:19302",
            "stun:stun2.l.google.com:19302"
        };
        
        // ICE transport policy
        public RTCIceTransportPolicy iceTransportPolicy = RTCIceTransportPolicy.All;
        
        // Get RTC configuration for peer connection
        public RTCConfiguration GetConfiguration()
        {
            var config = new RTCConfiguration
            {
                iceServers = new RTCIceServer[]
                {
                    new RTCIceServer { urls = iceServers }
                },
                iceTransportPolicy = iceTransportPolicy
            };
            
            return config;
        }
    }
}