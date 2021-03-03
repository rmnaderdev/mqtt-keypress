#include <WiFi.h>
#include <PubSubClient.h>
#include <Bounce2.h>


// WiFi network name and password:
const char *ssid = "XXXX";
const char *password = "XXXX";
const char *mqttServer = "X.X.X.X";

const int LED_PIN = 2;
const int BUTTON_PIN = 0;

WiFiClient wifiClient;
PubSubClient client(wifiClient);

Bounce2::Button button = Bounce2::Button();

void connectToWiFi(const char *ssid, const char *pwd)
{
	int ledState = 0;

	Serial.println();
	Serial.println("Connecting to WiFi network: " + String(ssid));

	WiFi.begin(ssid, pwd);

	while (WiFi.status() != WL_CONNECTED)
	{
		// Blink LED while we're connecting:
		digitalWrite(LED_PIN, ledState);
		ledState = (ledState + 1) % 2; // Flip ledState
		delay(500);
		Serial.print(".");
	}

	Serial.println();
	Serial.println("WiFi connected!");
	Serial.print("IP address: ");
	Serial.println(WiFi.localIP());
}

void callback(char *topic, byte *payload, unsigned int length)
{
	Serial.print("Message arrived [");
	Serial.print(topic);
	Serial.print("] ");
	for (int i = 0; i < length; i++)
	{
		Serial.print((char)payload[i]);
	}
	Serial.println();
}

void reconnect()
{
	// Loop until we're reconnected
	while (!client.connected())
	{
		Serial.println("Attempting MQTT connection...");
		// Attempt to connect
		if (client.connect("DeafenRemote"))
		{
			Serial.println("Connected!");
		}
		else
		{
			Serial.print("failed, rc=");
			Serial.print(client.state());
			Serial.println(" try again in 5 seconds");
			// Wait 5 seconds before retrying
			delay(5000);
		}
	}
}

void setup()
{
	Serial.begin(115200);
	pinMode(LED_PIN, OUTPUT);

	button.attach(BUTTON_PIN, INPUT_PULLUP);
	button.interval(5);
	button.setPressedState(LOW);

	// Connect to the WiFi network (see function below loop)
  	connectToWiFi(ssid, password);

	client.setServer(mqttServer, 2000);
	client.setCallback(callback);
}

void loop()
{
	button.update();

	if(button.pressed())
	{
		Serial.println("Sending deafen");
		client.publish("Test", "deafen");
	}

	if (!client.connected())
	{
		reconnect();
	}
	client.loop();
}