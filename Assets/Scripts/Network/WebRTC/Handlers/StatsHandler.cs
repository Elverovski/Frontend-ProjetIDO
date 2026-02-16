using System;
using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Interfaces;

namespace Network.WebRTC.Handlers
{
    public class StatsHandler : IStatsHandler
    {
        private readonly IRtcClient rtcClient;
        private readonly MonoBehaviour coroutineRunner;
        private Coroutine statsCoroutine;

        public event Action<RTCStatsReport> OnStatsUpdated;

        public RTCStatsReport LatestStats { get; private set; }

        private bool isMonitoring;
        private float updateInterval = 1f;

        public StatsHandler(IRtcClient client, MonoBehaviour runner)
        {
            rtcClient = client;
            coroutineRunner = runner;
        }

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
                    }
                }

                yield return new WaitForSeconds(updateInterval);
            }
        }

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

        public double GetVideoBitrate()
        {
            var inboundRtp = GetStatByType(RTCStatsType.InboundRtp);
            if (inboundRtp != null && inboundRtp.Dict.ContainsKey("bytesReceived"))
            {
                return 0;
            }
            return 0;
        }

        public void Dispose()
        {
            StopMonitoring();
        }
    }
}