#include "DHT.h"
#include "MHZ19_uart.h"
#include "UBCLib.h"
#define LEDPIN 13   // Indicator led pin
#define DHTPIN 2    // DHT sensor pin
#define DHTTYPE DHT11
#define MHZRXPIN 4  // mh-z19
#define MHZTXPIN 3  // mh-z19
#define MHZPWMPIN 5 // mh-z19
#define RSTPIN 12

DHT dht(DHTPIN, DHTTYPE);
MHZ19_uart mhz19;
UBCSerial ubc;

void setup() {
  Serial.begin(115200);

  // bug with this
  //pinMode(RSTPIN, OUTPUT);
  //digitalWrite(RSTPIN, HIGH);
  
  pinMode(LEDPIN, OUTPUT);
  digitalWrite(LEDPIN, HIGH);

  // dht init
  dht.begin();

  // mhz19 init
  int status;
  mhz19.setAutoCalibration(true);
  mhz19.begin(MHZRXPIN, MHZTXPIN);
  
  status = mhz19.getStatus();
  delay(200);
  
  status = mhz19.getStatus();
  delay(200);
  
  ubc.onMessageReceived(receive);
  digitalWrite(LEDPIN, LOW);
}

void loop()
{
  ubc.read();
}

void receive(Message message) {
  if (message.command == "dht11_dt") {
    getDht11Data();
  } else if (message.command == "mhz19_dt") {
    getMhz19Data();
  } else if (message.command == "ping") {
    Serial.print("pong");
  } 
}

void getDht11Data() {
  digitalWrite(LEDPIN, HIGH);
  
  float t = dht.readTemperature();
  float h = dht.readHumidity();

  digitalWrite(LEDPIN, LOW);
  
  if(!isnan(t) && !isnan(t)) {
   Serial.print("dht11:t=");
   Serial.print(t);
    
   Serial.print(";h=");
   Serial.print(h);
   Serial.println(";");
  }
  else {
   Serial.print("dht11:t=0;h=0;");
  }

}

void getMhz19Data() {
  digitalWrite(LEDPIN, HIGH);

  int c = mhz19.getPPM();
  
  digitalWrite(LEDPIN, LOW);

  if(!isnan(c) && c > 0) {
    Serial.print("mh-z19:co2=");
    Serial.print(c);
    Serial.println(";");
  } else {
    Serial.print("mh-z19:co2=0;");
  }
}
