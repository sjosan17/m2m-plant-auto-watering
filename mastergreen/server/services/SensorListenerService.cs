using mastergreen.server.azure;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mastergreen.server.services
{
    /// <summary>
    /// Gets the data directly from the Azure IoT hub.
    /// </summary>
    public class SensorListenerService : IHostedService
    {

        private readonly ISubscriptionService _subService;

        public SensorListenerService(ISubscriptionService subService)
        {
            _subService = subService;
        }
        private EventProcessorHost eventProcessorHost { get; set; }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            eventProcessorHost = new EventProcessorHost(
          "plantgrow",
          "realtimeweb",
          "Azure-IoT-hub-endpoint-url"
          , "Azure-Storage_account-Access_keys-connection_string-url", "data1");

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorFactoryAsync(new AzureStreamProcessorFactory(_subService),new EventProcessorOptions
            {
                InitialOffsetProvider = (pid) => EventPosition.FromEnqueuedTime(DateTime.UtcNow)
            });

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}
