using mastergreen.server.database;
using System;

namespace mastergreen.server.services
{
    public interface ISubscriptionService
    {
        void Subscribe(EventHandler handler);
        void SensorDataRecievied(SensorData sensorData);

    }
    public class SubscriptionService : ISubscriptionService
    {

        private EventHandler OnSensorData { get; set; }
        public void SensorDataRecievied(SensorData sensorData)
        {
            var sensorArgs = new SensorArgs
            {
                data = sensorData
            };
            OnSensorData?.Invoke(this, sensorArgs);
        }

        public void Subscribe(EventHandler handler)
        {
            OnSensorData += handler;
        }
    }
    public class SensorArgs : EventArgs  {
        public SensorData data { get; set; }
    }
}