using System;
using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Core;

namespace Network.WebRTC.Handlers
{
    /// <summary>
    /// Handles WebRTC statistics monitoring
    /// </summary>
    public class StatsHandler
    {
        private RTCClient rtcClient;
        private MonoBehaviour coroutineRunner;
        private Coroutine statsCoroutine;

        // Event for updated stats
        public event Action<RTCStatsReport> OnStatsUpdated;

        // Latest stats
        public RTCStatsReport LatestStats { get; private set; }

        private bool isMonitoring = false;
        private float updateInterval = 1f;

        public StatsHandler(RTCClient client, MonoBehaviour runner)
        {
            rtcClient = client;
            coroutineRunner = runner;
        }

        // Start monitoring stats
        public void StartMonitoring(float interval = 1f)
        {
            if (isMonitoring)
            {
                Debug.LogWarning("[STATS] Already monitoring");
                return;
            }

            updateInterval = interval;
            isMonitoring = true;

            statsCoroutine = coroutineRunner.StartCoroutine(MonitorStatsRoutine());
            Debug.Log("[STATS] Started monitoring");
        }

        // Stop monitoring stats
        public void StopMonitoring()
        {
            if (!isMonitoring) return;

            isMonitoring = false;

            if (statsCoroutine != null)
            {
                coroutineRunner.StopCoroutine(statsCoroutine);
                statsCoroutine = null;
            }

            Debug.Log("[STATS] Stopped monitoring");
        }

        // Coroutine to periodically fetch stats
        private IEnumerator MonitorStatsRoutine()
        {
            while (isMonitoring)
            {
                var peerConnection = rtcClient.GetPeerConnection();

                if (peerConnection != null)
                {
                    var statsOp = peerConnection.GetStats();
                    yield return statsOp;

                    if (!statsOp.IsError)
                    {
                        LatestStats = statsOp.Value;
                        OnStatsUpdated?.Invoke(LatestStats);
                        // Debug.Log($"[STATS] Updated: {LatestStats.Stats.Count} stats");
                    }
                }

                yield return new WaitForSeconds(updateInterval);
            }
        }

        // Get a specific stat by type
        public RTCStats GetStatByType(RTCStatsType type)
        {
            if (LatestStats == null) return null;

            foreach (var stat in LatestStats.Stats.Values)
            {
                if (stat.Type == type)
                {
                    return stat;
                }
            }

            return null;
        }

        // Get video bitrate 
        public double GetVideoBitrate()
        {
            var inboundRtp = GetStatByType(RTCStatsType.InboundRtp);
            if (inboundRtp != null && inboundRtp.Dict.ContainsKey("bytesReceived"))
            {
                // TODO: implement bitrate calculation
                return 0;
            }
            return 0;
        }

        // Stop monitoring and cleanup
        public void Dispose()
        {
            StopMonitoring();
        }
    }
}
