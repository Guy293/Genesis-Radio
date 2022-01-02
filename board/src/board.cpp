// TODO: Bluetooth Serial backlog


/*
 * LoRa E22
 * set configuration.
 * https://www.mischianti.org
 *
 * E22      ----- esp32
 * M0         ----- 19 (or GND)
 * M1         ----- 21 (or 3.3v)
 * RX         ----- TX2 (PullUP)
 * TX         ----- RX2 (PullUP)
 * AUX        ----- 15  (PullUP)
 * VCC        ----- 3.3v/5v
 * GND        ----- GND
 *
 */
#include "Arduino.h"
#include "EBYTE.h"

// #include <WiFi.h>
// #include <ESPmDNS.h>
// #include <WiFiUdp.h>
// #include <ArduinoOTA.h>

// #include <ArduinoJson.h>
#include "BluetoothSerial.h"

BluetoothSerial SerialBT;

// #define WiFiOTA false

// #define SerialDebug SerialBT
#define SerialDebug Serial
#define blinkEnabled false

#define PIN_RX 16
#define PIN_TX 17
#define PIN_AUX 18
#define PIN_M0 21
#define PIN_M1 19

#define PIN_LED 2

// i recommend putting this code in a .h file and including it
// from both the receiver and sender modules
struct DATA
{
    String Message;
    int count;
};

DATA dataSend;
unsigned long Last;
unsigned int ledOnUntil;
bool ledStatus;

// create the transceiver object, passing in the serial and pins
EBYTE Transceiver(&Serial2, PIN_M0, PIN_M1, PIN_AUX);

void setup()
{
    Serial.begin(9600);
    delay(500);
    Serial.println();

    SerialBT.begin("ESP32-Lora");
    SerialDebug.println("Bluetooth started");

    pinMode(PIN_LED, OUTPUT);

    Serial2.begin(9600);
    SerialDebug.println("Lora Module Started");

    Transceiver.init();

    Transceiver.SetTransmitRSSI(1);
    // Transceiver.SetTransmissionMode(0);
    Transceiver.SetUARTBaudRate(0x3);
    Transceiver.SetAirDataRate(0x2);
    Transceiver.SetAddressL(0x0);
    Transceiver.SetAddressH(0x0);
    Transceiver.SetListenBeforeTransmit(0x1);
    // Transceiver.SetChannel(0x0);
    Transceiver.SetChannel(0x3C);
    Transceiver.SaveParameters();

    Transceiver.PrintParameters();

    Last = millis();
}

void loop()
{

    // delay(1000);

    // return;

    if (ledOnUntil >= millis())
    {
        if (!ledStatus)
        {
            digitalWrite(PIN_LED, HIGH);
            ledStatus = true;
        }
    }
    else
    {
        if (ledStatus)
        {
            digitalWrite(PIN_LED, LOW);
            ledStatus = false;
        }
    }

    if (Serial2.available())
    {
        DATA dataRecv;
        Transceiver.GetStruct(&dataRecv, sizeof(dataRecv));

        char rssi[1];
        Serial2.readBytes(rssi, 1);

        SerialDebug.printf("%s | RSSI: %d\n", dataRecv.Message, rssi[0]);

        if (blinkEnabled)
        {
            ledOnUntil = millis() + 100;
        }


        dataRecv.count++;
        delay(500);
        Transceiver.SendStruct(&dataRecv, sizeof(dataRecv));
        Last = millis();
    }
    else if ((millis() - Last) > 5000)
    {
        SerialDebug.println("No response in the last 10 seconds... Sending initial message");
        DATA dataSend;
        dataSend.Message = "hello!";
        dataSend.count = 0;
        Transceiver.SendStruct(&dataSend, sizeof(dataSend));

        Last = millis();
    }
}