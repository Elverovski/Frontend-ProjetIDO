using System;
using System.Text;
using UnityEngine;
using Unity.WebRTC;
using Network.WebRTC.Interfaces;
using Network.WebRTC.Models;

namespace Network.WebRTC.Handlers
{
    public class DataChannelHandler : IDataChannelHandler
    {
        private readonly IRtcClient rtcClient;
        private RTCDataChannel commandChannel;
        private RTCDataChannel telemetryChannel;

        public event Action<TelemetryData> OnTelemetryReceived;
        public event Action OnDataChannelReady;

        private bool isReady;
        public bool IsReady => isReady;

        public DataChannelHandler(IRtcClient client)
        {
            rtcClient = client;
            rtcClient.OnDataChannel += HandleIncomingDataChannel;
        }

        public void CreateDataChannels()
        {
            Debug.Log("[DATA CHANNEL] Creating data channels");

            commandChannel = rtcClient.CreateDataChannel("commands");
            SetupDataChannel(commandChannel, "COMMAND");

            telemetryChannel = rtcClient.CreateDataChannel("telemetry");
            SetupDataChannel(telemetryChannel, "TELEMETRY");
        }

        private void HandleIncomingDataChannel(RTCDataChannel channel)
        {
            Debug.Log($"[DATA CHANNEL] Incoming channel: {channel.Label}");

            switch (channel.Label)
            {
                case "commands":
                    commandChannel = channel;
                    SetupDataChannel(commandChannel, "COMMAND");
                    break;
                case "telemetry":
                    telemetryChannel = channel;
                    SetupDataChannel(telemetryChannel, "TELEMETRY");
                    break;
            }
        }

        private void SetupDataChannel(RTCDataChannel channel, string channelType)
        {
            channel.OnOpen = () => 
            {
                Debug.Log($"[DATA CHANNEL] {channelType} channel opened");
                CheckIfReady();
            };

            channel.OnClose = () => 
            {
                Debug.Log($"[DATA CHANNEL] {channelType} channel closed");
                isReady = false;
            };

            channel.OnMessage = (bytes) => 
            {
                if (channelType == "TELEMETRY")
                {
                    HandleTelemetryMessage(bytes);
                }
            };
        }

        private void CheckIfReady()
        {
            if (commandChannel?.ReadyState == RTCDataChannelState.Open &&
                telemetryChannel?.ReadyState == RTCDataChannelState.Open)
            {
                isReady = true;
                Debug.Log("[DATA CHANNEL] All channels ready");
                OnDataChannelReady?.Invoke();
            }
        }

        private void HandleTelemetryMessage(byte[] bytes)
        {
            try
            {
                string json = Encoding.UTF8.GetString(bytes);
                TelemetryData telemetry = JsonUtility.FromJson<TelemetryData>(json);
                
                OnTelemetryReceived?.Invoke(telemetry);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DATA CHANNEL] Failed to parse telemetry: {ex.Message}");
            }
        }

        public void SendCommand(string jsonCommand)
        {
            if (commandChannel == null || commandChannel.ReadyState != RTCDataChannelState.Open)
            {
                Debug.LogWarning("[DATA CHANNEL] Command channel not ready");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(jsonCommand);
                commandChannel.Send(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DATA CHANNEL] Failed to send command: {ex.Message}");
            }
        }

        public void Dispose()
        {
            commandChannel?.Close();
            telemetryChannel?.Close();
            
            rtcClient.OnDataChannel -= HandleIncomingDataChannel;
        }
    }
}