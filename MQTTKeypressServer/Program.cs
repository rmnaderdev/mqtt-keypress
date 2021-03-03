using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace MQTTKeypressServer
{
    public class Program
    {
        private static readonly ILogger Logger = Log.ForContext<Program>();


        public static void Main(string[] args)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Logging setup
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(currentPath, @"log\MQTT_Server_.txt"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();


            // MQTT server setup
            var optionsBuilder = new MqttServerOptionsBuilder()
                // Networking
                .WithDefaultEndpoint().WithDefaultEndpointPort(2000)
                .WithConnectionValidator(c =>
                {
                    Logger.Information("{user} Connected!", new { user = c.ClientId });

                    c.ReasonCode = MqttConnectReasonCode.Success;
                })
                .WithSubscriptionInterceptor(c =>
                {
                    c.AcceptSubscription = true;
                })
                .WithApplicationMessageInterceptor(c =>
                {
                    c.AcceptPublish = true;
                    var content = Encoding.UTF8.GetString(c.ApplicationMessage.Payload);

                    switch(content)
                    {
                        case "deafen":
                            new InputSimulator().Keyboard.KeyPress(VirtualKeyCode.F20);
                            break;
                    }


                    Logger.Information("Got payload from {user} with topic={topic}, content={content}", new { user = c.ClientId, topic = c.ApplicationMessage.Topic, content = content });
                })
                .Build();

            Logger.Information("Starting MQTT Server...");
            var mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder).Wait();
            Logger.Information("Started MQTT Server!");

            // Close program on enter press
            Console.ReadLine();
        }
    }
}
