using System;

namespace Network.WebRTC.Models
{
    [Serializable]
    public class TelemetryData
    {
        public BatteryData battery;
        public GPSData gps;
        public SensorData sensors;
        public SystemData system;
        public long timestamp;
    }

    [Serializable]
    public class BatteryData
    {
        public float voltage;
        public float current;
        public float percentage;
        public float temperature;
        public string status;
    }

    [Serializable]
    public class GPSData
    {
        public double latitude;
        public double longitude;
        public float altitude;
        public float speed;
        public float heading;
        public int satellites;
        public float accuracy;
    }

    [Serializable]
    public class SensorData
    {
        public float[] accelerometer;
        public float[] gyroscope;
        public float[] magnetometer;
        public float temperature;
        public float humidity;
    }

    [Serializable]
    public class SystemData
    {
        public float cpuUsage;
        public float memoryUsage;
        public float cpuTemperature;
        public int signalStrength;
        public string networkType;
    }
}