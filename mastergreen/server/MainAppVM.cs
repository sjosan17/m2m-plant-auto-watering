using DotNetify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using mastergreen.server.database;
using mastergreen.server.services;

namespace mastergreen.server
{
    public class MainAppVM : BaseVM
    {
        public List<SensorData> Devices { get; set; }

        private Timer _timer;
        private readonly ISubscriptionService _subService;
        //private ServiceClient s_serviceClient;

        
        // Connection string for your IoT Hub
        // az iot hub show-connection-string --hub-name {your iot hub name} --policy-name service
        public static ServiceClient s_serviceClient = ServiceClient.CreateFromConnectionString("Azure-Iot hub-Shared acced policies-iothubowner-connectionstring-url");
        
        public string Greetings => "Hello World!";
        public DateTime ServerTime => DateTime.Now;

        // Sends realtime data
        public MainAppVM(ISubscriptionService subscriptionService)
        {
            _subService = subscriptionService;
            _subService.Subscribe(OnData);
            Devices = new List<SensorData>();
            
            var DeviceData = new DeviceDataProvider();
            var data = DeviceData.GetData();
            var devices = data.Documents.GroupBy(e => e.deviceId);
            foreach (var device in devices)
            {
                Devices.Add(device.FirstOrDefault());
            }

            _timer = new Timer(state =>
            {
                Changed(nameof(ServerTime));
                PushUpdates();
            }, null, 0, 1000);
        }
        
        // Sends a direct method to device via IoThub, Id to make sure correct plant is invoked.
        public Action<int> Water => async id =>
        {
            var methodInvocation = new CloudToDeviceMethod("fillWater") { ResponseTimeout = TimeSpan.FromSeconds(30) };
            methodInvocation.SetPayloadJson("0");

            // Invoke the direct method asynchronously and get the response from the simulated device.
            var response = await s_serviceClient.InvokeDeviceMethodAsync("Plant_1", methodInvocation);

            Console.WriteLine("Response status: {0}, payload:", response.Status);
            if (response.Status == 200)
            {
                // Send new time update to "Last pump: (Servertime)
            }
            Console.WriteLine(response.GetPayloadAsJson());
        };
        /// Realtime data push to frontend
        private void OnData(object sender, EventArgs e)
        {
            var args = (SensorArgs)e;
            var device = Devices.Where(e => e.deviceId == args.data.deviceId).FirstOrDefault();
            if (device != null)
            {
                device.SoilMoisture = args.data.SoilMoisture;
                device.Temperature = args.data.Temperature;
                device.Humidity = args.data.Humidity;
                device.Light = args.data.Light;
            }

            Changed(nameof(Devices));
            PushUpdates();
            
            Console.WriteLine(args.data.message);
        }
        public override void Dispose() => _timer.Dispose();
    }
}

