using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MQTTClientTest
{
    public partial class Form1 : Form
    {

        private IMqttClient client;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {

            var mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("MQTTClientTest")
                .WithTcpServer(txtIpAddress.Text, int.Parse(txtPort.Text.Trim()))
                .WithCleanSession()
                .Build();


            client = new MqttFactory().CreateMqttClient();


            client.UseApplicationMessageReceivedHandler(e =>
            {
                lstOutput.Items.Add(string.Format(">>> {0}", Encoding.UTF8.GetString(e.ApplicationMessage.Payload)));
            });

            client.UseDisconnectedHandler(async e =>
            {
                if(!e.ClientWasConnected || e.Reason == MqttClientDisconnectReason.NormalDisconnection)
                {
                    return;
                }

                MessageBox.Show("Unexpected disconnect from MQTT server. Trying again in 5 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    lstOutput.Items.Add("Connecting to MQTT...");
                    await client.ConnectAsync(mqttOptions);
                    lstOutput.Items.Add("Connected!");
                }
                catch
                {
                    lstOutput.Items.Add("Failed to reconnect to MQTT server.");
                }
            });

            // Trying on btn click
            try
            {
                lstOutput.Items.Add("Connecting to MQTT...");
                await client.ConnectAsync(mqttOptions);
                lstOutput.Items.Add("Connected!");
            }
            catch
            {
                MessageBox.Show("Failed to connect to MQTT server.");
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Disconnect();
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if(cbxDelay.Checked)
            {
                await Task.Delay(5000);
                await SendKey();
            }
            else
            {
                await SendKey();
            }
        }

        private async Task Disconnect()
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync();
            }
        }

        private async Task SendKey()
        {
            if (client.IsConnected)
            {
                if (!string.IsNullOrEmpty(txtInput.Text))
                {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic("Test")
                        .WithPayload(txtInput.Text)
                        .WithExactlyOnceQoS()
                        .WithRetainFlag()
                        .Build();

                    await client.PublishAsync(message);

                    lstOutput.Items.Add(string.Format("{0} <<<", txtInput.Text));
                }
            }
        }
    }
}
