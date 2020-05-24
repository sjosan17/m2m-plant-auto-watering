using mastergreen.server.services;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mastergreen.server.azure
{
    /// <summary>
    /// Creates a internal subription service which the realtime data will publish to.
    /// </summary>
    class AzureStreamProcessorFactory : IEventProcessorFactory
    {
        public AzureStreamProcessorFactory(ISubscriptionService subscriptionService)
        {
            _subService = subscriptionService;
        }
        private readonly ISubscriptionService _subService;
        private EventProcessorHost eventProcessorHost;

        IEventProcessor IEventProcessorFactory.CreateEventProcessor(PartitionContext context)
        {

            return new SensorReader(_subService);
        }
    }
}
