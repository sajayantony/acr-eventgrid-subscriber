using acr_eventgrid_subscriber.Commands;
using System.CommandLine;

namespace Azure.ACR.Sample.Server.Commands
{
    class ACRSubscriberRootCommand : RootCommand
    {
        public ACRSubscriberRootCommand()
        {
            this.AddGlobalOption(new Option<string>(
                aliases: new string[] { CommandGlobalOptions.QUEUE_NAME, "-q" },
                description: "Name of the queue to listen to"));


            this.AddGlobalOption(new Option<string>(
                    aliases: new string[] { CommandGlobalOptions.STORAGE_CONNECTION_STRING, "-c" },
                    description: "Storage account connection string"));

            this.Add(new SubscribeCommand());

        }
    }
}