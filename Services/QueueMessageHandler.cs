using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Azure.ACR.Sample.Services
{

    class StorageAccountCredentialHelper
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }

    class QueueMessageHandler
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly QueueClient _queueClient;
        private bool _stopping;

        public QueueMessageHandler(IHostApplicationLifetime lifetime,  
            ILoggerFactory loggerFactory, 
            StorageAccountCredentialHelper creds)
        {
            _logger = loggerFactory.CreateLogger(typeof(QueueMessageHandler));
            _connectionString = creds.ConnectionString;
            _queueName = creds.QueueName;
            _queueClient = new QueueClient(_connectionString, _queueName);
            lifetime.ApplicationStopping.Register(() =>
            {
                _stopping = true;
            });
        }

        public async Task RecieveMessagesAsync()
        {
            _logger.LogInformation("Starting receiver");

            while (!_stopping)
            {

                if (await _queueClient.ExistsAsync())
                {
                    // Get the next message
                    QueueMessage[] retrievedMessage = await _queueClient.ReceiveMessagesAsync();

                    foreach (var msg in retrievedMessage)
                    {
                        await HandleMessageAsync(msg);
                    }

                    continue;
                }

                //Wait for 1 second before polling
                await Task.Delay(1000);
            }

            _logger.LogInformation("Stopping Reciever.");
        }

        private async Task HandleMessageAsync(QueueMessage msg)
        {
            _logger.LogInformation($"Dequeued message: '{msg.MessageText}'");
            await _queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
        }
    }
}
