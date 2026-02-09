using System;
using Unity.WebRTC;

namespace Network.WebRTC.Core
{
    [Serializable]
    public class RTCConfig
    {
        public string[] iceServers = new string[]
        {
            "stun:stun.l.google.com:19302",
            "stun:stun1.l.google.com:19302",
            "stun:stun2.l.google.com:19302"
        };
        
        public RTCIceTransportPolicy iceTransportPolicy = RTCIceTransportPolicy.All;
        
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