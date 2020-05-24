#include <Arduino.h>
#include <Adafruit_Sensor.h>
#include <DHT.h>

#include <WiFi.h>
#include "AzureIotHub.h"
#include "Esp32MQTTClient.h"

// -- Azure - MQTT --------------------------------------
#define INTERVAL 10000
#define DEVICE_ID "Plant_1"
#define MESSAGE_MAX_LEN 256
// -- DHT - Sensor --------------------------------------
#define DHTTYPE                 DHT11                 // [Sensor] Change to DHT11, DHT21, DHT22
#define PIN_DHT                 22                    // [Sensor] Pin for DHT sensor
#define TEMP_UNIT               "C"                   // [Sensor] Temperature Unit (C = Celsius, F = Fahrenheit)

#define OFFSET_TEMPERATURE      0                     // [Offset] If you need to readjust the temperature value
#define OFFSET_HUMIDITY         0                     // [Offset] If you need to readjust the humidity value

// -- Other - Sensor ------------------------------------
#define PIN_SOIL                32                    // [Sensor] Soil Sensor pin
#define PIN_LIGHT               33                    // [Sensor] Light Sensor pin
#define PIN_POWER               34                    // [Sensor] Power Sensor pin
const int PIN_PUMP = 25;                              // [Motor] Water pump pin

// Initialize DHT sensor.
DHT dht(PIN_DHT, DHTTYPE);

// Please input the SSID and password of WiFi
const char* ssid     = ".....................";
const char* password = "....................";

/*String containing Hostname, Device Id & Device Key in the format:                         */
/*  "HostName=<host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"                */
/*  "HostName=<host_name>;DeviceId=<device_id>;SharedAccessSignature=<device_sas_token>"    */
static const char* connectionString = ".......................................................";

//Structure of msg to IoT Hub
const char *messageData = "{\"deviceId\":\"%s\", \"SoilMoisture\":%f, \"Temperature\":%f, \"Humidity\":%f, \"Light\":%f}";

static bool hasWifi = false;
static bool messageSending = true;
static uint64_t send_interval_ms;


////////////////////////////////////////////////////////////////////////////////////////////////////////// Utilities
static void InitWifi()
{
  Serial.println("Connecting...");
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  hasWifi = true;
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

static void SendConfirmationCallback(IOTHUB_CLIENT_CONFIRMATION_RESULT result)
{
  if (result == IOTHUB_CLIENT_CONFIRMATION_OK)
  {
    Serial.println("Send Confirmation Callback finished.");
  }
}

static void MessageCallback(const char* payLoad, int size)
{
  Serial.println("Message callback:");
  Serial.println(payLoad);
}

static void DeviceTwinCallback(DEVICE_TWIN_UPDATE_STATE updateState, const unsigned char *payLoad, int size)
{
  char *temp = (char *)malloc(size + 1);
  if (temp == NULL)
  {
    return;
  }
  memcpy(temp, payLoad, size);
  temp[size] = '\0';
  // Display Twin message.
  Serial.println(temp);
  free(temp);
}
// Runs the DC pump for 1 and half second.
void pump(){
  Serial.println("Now Pumping...");
  digitalWrite(PIN_PUMP, HIGH);
  delay(1500);
  digitalWrite(PIN_PUMP, LOW);
  delay(1500);
}

// Use for Direct Method from Azure IoT Hub
static int  DeviceMethodCallback(const char *methodName, const unsigned char *payload, int size, unsigned char **response, int *response_size)
{
  LogInfo("Try to invoke method %s", methodName);
  const char *responseMessage = "\"Successfully invoke device method\"";
  int result = 200;

  if (strcmp(methodName, "start") == 0)
  {
    LogInfo("Start sending plant data");
    messageSending = true;
  }
  else if (strcmp(methodName, "stop") == 0)
  {
    LogInfo("Stop sending plant data");
    messageSending = false;
  }
  else if (strcmp(methodName, "fillWater") == 0)
  {
    LogInfo("Start pump");
    pump();
    Serial.println("Invoke pump method");
    result = 200;
  }
  else
  {
    LogInfo("No method %s found", methodName);
    responseMessage = "\"No method found\"";
    result = 404;
  }

  *response_size = strlen(responseMessage) + 1;
  *response = (unsigned char *)strdup(responseMessage);

  return result;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Arduino body


void setup() {
  dht.begin(); // initialize the DHT sensor
  pinMode(PIN_PUMP, OUTPUT); // set pin output for water pump

  //Initialize
  Serial.begin(115200);

  Serial.begin(115200);
  Serial.println("Plant_1 Device");
  Serial.println("Initializing...");

  // Initialize the WiFi module
  Serial.println(" > WiFi");
  hasWifi = false;
  InitWifi();
  if (!hasWifi)
  {
    return;
  }
  randomSeed(analogRead(0));

  Serial.println(" > IoT Hub");
  Esp32MQTTClient_SetOption(OPTION_MINI_SOLUTION_NAME, "GetStarted");
  Esp32MQTTClient_Init((const uint8_t*)connectionString, true);

  Esp32MQTTClient_SetSendConfirmationCallback(SendConfirmationCallback);
  Esp32MQTTClient_SetMessageCallback(MessageCallback);
  Esp32MQTTClient_SetDeviceTwinCallback(DeviceTwinCallback);
  Esp32MQTTClient_SetDeviceMethodCallback(DeviceMethodCallback);

  send_interval_ms = millis();

}

// Read the water level and return a readable value(from 3000 something to sub 1000)
float getWater() {
  float water = analogRead(PIN_SOIL);
  water = map(water, 0, 4095, 0, 1023);
  water = constrain(water, 0, 1023);
  return water;
}

// Read the light level and return a readable value
float getLight() {
  float light = analogRead(PIN_LIGHT);
  light = map(light, 0, 4095, 0, 1023);
  light = constrain(light, 0, 1023);
  return light;
}

// Read the temperature and return a readable value + can easly change to include Fahrenheit
float getTemperature() {
  float temperature = dht.readTemperature();
  temperature += OFFSET_TEMPERATURE;
  return temperature;
}

// Read the humidity level and return a readable value
float getHumidity() {
  return (float)dht.readHumidity() + OFFSET_HUMIDITY;
}


void loop() {
  // Debug data to check if sensors are working with no internett.
  Serial.println();
  Serial.print("Soil Moisture: ");
  Serial.print(getWater());
  Serial.println();
  Serial.print("Humidity: ");
  Serial.print(getHumidity());
  Serial.println();
  Serial.print("Temperature: ");
  Serial.print(getTemperature());
  Serial.println();
  Serial.print("Light: ");
  Serial.print(getLight());

  // Production data sent to Azure IoT Hub
  if (hasWifi)
  {
    if (messageSending && 
        (int)(millis() - send_interval_ms) >= INTERVAL)
    {
      // Send plant data
      char messagePayload[MESSAGE_MAX_LEN];
      snprintf(messagePayload,MESSAGE_MAX_LEN, messageData, DEVICE_ID, getWater(), getTemperature(),getHumidity(), getLight());
      Serial.println(messagePayload);
      EVENT_INSTANCE* message = Esp32MQTTClient_Event_Generate(messagePayload, MESSAGE);
      Esp32MQTTClient_Event_AddProp(message, "plantdata", "true");
      Esp32MQTTClient_SendEventInstance(message);
      
      send_interval_ms = millis();
    }
    else
    {
      Esp32MQTTClient_Check();
    }
  }
  delay(10000);
}