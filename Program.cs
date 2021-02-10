using Azure.ACR.Sample.Server.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Azure.ACR.Sample.Services
{
    class Program
    {
        static async Task Main(string[] args)
        {            
            var cmd = new ACRSubscriberRootCommand();
            var builder = new CommandLineBuilder(cmd);

            await builder.UseHost(_ => Host.CreateDefaultBuilder(),
                    host =>
                    {
                        
                        InvocationContext context = (InvocationContext)host.Properties[typeof(InvocationContext)];
                        
                        host.ConfigureServices(services =>
                        {
                            var connectionString = context.ParseResult.ValueForOption<string>(CommandGlobalOptions.STORAGE_CONNECTION_STRING);
                            var queueName = context.ParseResult.ValueForOption<string>(CommandGlobalOptions.QUEUE_NAME);

                            services.AddSingleton<StorageAccountCredentialHelper>((provider) =>
                            {
                                return new StorageAccountCredentialHelper()
                                {
                                    ConnectionString = OverrideEnvWithArgs(connectionString, CommandGlobalOptions.STORAGE_CONNECTION_STRING_ENV),
                                    QueueName = OverrideEnvWithArgs(queueName, CommandGlobalOptions.QUEUE_NAME_ENV)
                                };
                            });

                            services.AddScoped<QueueMessageHandler>();
                            
                        });
                    })                
                .UseVersionOption()
                .UseHelp()
                .UseEnvironmentVariableDirective()
                .UseDebugDirective()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseTypoCorrections()
                .UseParseErrorReporting()
                .CancelOnProcessTermination()
                .Build()
                .InvokeAsync(args);
        }


        static string OverrideEnvWithArgs(string val, string env)
        {
            Console.WriteLine(val);
            if (!string.IsNullOrEmpty(val))
            {
                return val;
            }

            return Environment.GetEnvironmentVariable(env);
        }
    }
}
