using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using DataAvailableNotification;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;

namespace QueueSender
{
    public static class Program
    {
        private static ServiceBusClient _client;
        private static ServiceBusSender _sender;

        public static async Task Main()
        {
            var configuration = BuidConfiguration();
            var connectionString = configuration.GetSection("Values")["ServiceBusConnectionString"];
            var queueName = configuration.GetSection("Values")["DataAvailableQueueName"];

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);

            using var messageBatch = await _sender.CreateMessageBatchAsync().ConfigureAwait(false);

            var msg = DataAvailableModel.CreateProtoContract(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), SubDomainOrigin.Charges);
            var bytearray = msg.ToByteArray();

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(new BinaryData(bytearray))))
            {
                throw new Exception("The message is too large to fit in the batch.");
            }
            else
            {
                Console.WriteLine($"Message added to batch, uuid: {msg.UUID}, recipient: {msg.Recipient} ");
            }

            try
            {
                await _sender.SendMessagesAsync(messageBatch).ConfigureAwait(false);
                Console.WriteLine($"A batch of messages has been published to the queue.");
            }
            finally
            {
                await _sender.DisposeAsync().ConfigureAwait(false);
                await _client.DisposeAsync().ConfigureAwait(false);
            }

            Console.WriteLine("Press any key to end the application");

            Console.ReadKey();
        }

        private static IConfiguration BuidConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", false, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            return configuration;
        }
    }
}
