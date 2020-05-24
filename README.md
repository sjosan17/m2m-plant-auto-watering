# m2m-plant-auto-watering
This is a end to end solution scaled to 10 000 plants. Each plant will have a HiGrow sensor which have soil moisture, temperate and air moisture built in. It will also have attached an analog light sensor. Together with these sensors the solution will get the data it need to water as needed.

If the water pump does not manage to increase the soil moisture, then the user will get a warning on email to refill the water.

The solution uses Microsoft Azure in backend and React for the frontend web app. Processing of all data happens with the ELK stack. ELK is ElasticSearch, Logstash and Kibana. This stack runs on a Azure VM server. The server handles all the data and shows it to the user with the web app.

# Hardware used:
male-male cables\
male-female cables\
1x 1k Ohm resistor\
1x 100k Ohm resistor\
1x light dependent resistor\
1x tip-120 transistor\
1x 1N4001 diode\
1x 3-6V dc water pump\
1x HiGrow Esp32\
1x Breadboard\

See the build in fritzing folder.

# Software used:
VS code with PlatformIO\
Azure IoT Hub\
1x Ubuntu VM with ELk and Nginx(security)\
Hosting plan for web app\
Storage account for realtime data\
Sendgrid account for warning on email\
Function Apps for 24/7 trigger function for watering and warning\
