#include "DHT.h"
#define LEDPIN 13 // Indicator led pin
#define DHTPIN 2 // DHT sensor pin
DHT dht(DHTPIN, DHT11);  // Initialize sensor

void setup() {
  pinMode(LEDPIN, OUTPUT);
  Serial.begin(9600);
  dht.begin();
}

void loop() {
  delay(1000);
  
  digitalWrite(LEDPIN, HIGH);

  float t = dht.readTemperature();
  float h = dht.readHumidity();

  // todo - co2
  int c = 800;

  digitalWrite(LEDPIN, LOW);


  if(!isnan(c)) {
    Serial.print("mh-z19b:co2=");
    Serial.print(c);
    Serial.println(";");
      
    delay(4500);
  } else {
     Serial.println("mh-z19b:co2=0;");
  }

  if(!isnan(h) && !isnan(t)) {
      Serial.print("dht11:t=");
      Serial.print(t);
      Serial.print(";h=");
      Serial.print(h);
      Serial.println(";");
      
      delay(4500);
  } else {
     Serial.println("dht11:t=0;h=0;");
  }

  
}
