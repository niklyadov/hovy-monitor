#include "DHT.h"
#include <Arduino.h>
#include "MHZ19_uart.h"

#define LEDPIN 13   // Indicator led pin
#define DHTPIN 2    // DHT sensor pin
#define MHZRXPIN 4  // mh-z19
#define MHZTXPIN 3  // mh-z19
#define MHZPWMPIN 5 // mh-z19
DHT dht(DHTPIN, DHT11);
MHZ19_uart mhz19;
 
void setup()
{
  pinMode(LEDPIN, OUTPUT);
  digitalWrite(LEDPIN, HIGH);
  
  int status;
 
  Serial.begin(9600);
 
  mhz19.begin(MHZRXPIN, MHZTXPIN);
  dht.begin();
  mhz19.setAutoCalibration(false);
  
  status = mhz19.getStatus();
  delay(500);
  
  status = mhz19.getStatus();
  delay(500);

  digitalWrite(LEDPIN, LOW);
}
 
void loop() {
  delay(3000);
  
  digitalWrite(LEDPIN, HIGH);

  float t = dht.readTemperature();
  float h = dht.readHumidity();
  int c = mhz19.getPPM();

  digitalWrite(LEDPIN, LOW);

  if(!isnan(c) && c > 0) {
    Serial.print("mh-z19:co2=");
    Serial.print(c);
    Serial.println(";");

  } else {
     Serial.println("mh-z19:co2=0;");
  }

  if(!isnan(h) && !isnan(t)) {
      Serial.print("dht11:t=");
      Serial.print(t);
      Serial.print(";h=");
      Serial.print(h);
      Serial.println(";");

  } else {
     Serial.println("dht11:t=0;h=0;");
  }

  
}
