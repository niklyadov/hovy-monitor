#include "DHT.h"
#include <Arduino.h>
#include "MHZ19_uart.h"

#define LEDPIN 13   // Indicator led pin
#define DHTPIN 2    // DHT sensor pin
#define DHTTYPE DHT11
#define MHZRXPIN 4  // mh-z19
#define MHZTXPIN 3  // mh-z19
#define MHZPWMPIN 5 // mh-z19
#define RSTPIN 12
DHT dht(DHTPIN, DHTTYPE);
MHZ19_uart mhz19;

int errorsCount = 0;
 
void setup()
{
  errorsCount = 0;
  
  pinMode(LEDPIN, OUTPUT);
  digitalWrite(LEDPIN, HIGH);

  pinMode(RSTPIN, OUTPUT);
  digitalWrite(RSTPIN, HIGH);
  
  int status;
 
  Serial.begin(9600);
 
  mhz19.begin(MHZRXPIN, MHZTXPIN);
  dht.begin();
  mhz19.setAutoCalibration(true);
  
  status = mhz19.getStatus();
  delay(500);
  
  status = mhz19.getStatus();
  delay(500);

  digitalWrite(LEDPIN, LOW);
}
 
void loop() {
  delay(5000);
  
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
     errorsCount++;
  }

  if(!isnan(t) && !isnan(t)) {
     Serial.print("dht11:t=");
     Serial.print(t);
      
     Serial.print(";h=");
     Serial.print(h);
     Serial.println(";");
  }
 else {
     Serial.println("dht11:t=0;h=0;");
     errorsCount++;
  }

  if(mhz19.getStatus() == -1) {
    errorsCount++;
  }

  if(errorsCount > 3) {
    Serial.end();
    digitalWrite(RSTPIN, LOW);
  }
}
