using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace MQTTKeypressServer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // MQTT server setup
            var optionsBuilder = new MqttServerOptionsBuilder()
                // Networking
                .WithDefaultEndpoint().WithDefaultEndpointPort(2000)
                .WithConnectionValidator(c =>
                {
                    _logger.LogInformation("{user} Connected!", new { user = c.ClientId });

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

                    switch (content)
                    {
                        case "deafen":
                            new InputSimulator().Keyboard.KeyPress(VirtualKeyCode.F20);
                            break;
                    }


                    _logger.LogInformation("Got payload from {user} with topic={topic}, content={content}", new { user = c.ClientId, topic = c.ApplicationMessage.Topic, content = content });
                })
                .Build();

            _logger.LogInformation("Starting MQTT Server...");
            var mqttServer = new MqttFactory().CreateMqttServer();

            await mqttServer.StartAsync(optionsBuilder);

            _logger.LogInformation("Started MQTT Server!");
        }
    }
}
