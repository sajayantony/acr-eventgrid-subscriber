using Azure.ACR.Sample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace acr_eventgrid_subscriber.Commands
{
    class SubscribeCommand : Command
    {
        public SubscribeCommand() : base("subscribe", "Start Subscribing to messages from storage")
        {            
            this.Handler = CommandHandler.Create<IHost>(async (host) =>
            {                
                var creds = host.Services.GetRequiredService<StorageAccountCredentialHelper>();
                if (string.IsNullOrEmpty(creds.ConnectionString))
                {
                    var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SubscribeCommand));                    
                    logger.LogError("No storage credentials available");
                    return -1;
                }

                var handler = host.Services.GetRequiredService<QueueMessageHandler>();
                await handler.RecieveMessagesAsync();
                return 0;
            });
        }
    }
}
