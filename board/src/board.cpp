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

#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>

#define SERVICE_UUID "16f88c52-1471-4bba-95a8-17094b0520d3"
#define NEW_MESSAGE_CHARACTERISTIC_UUID "af77d21b-1a5c-4910-b4b4-c98220ac0e79"

#define BLE_DEVICE_NAME "ESP32-Lora"

// #define WiFiOTA false

#define SerialDebug Serial
#define blinkEnabled true

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
};

DATA dataSend;
unsigned long Last;
unsigned int ledOnUntil;
bool ledStatus;

// create the transceiver object, passing in the serial and pins
EBYTE Transceiver(&Serial2, PIN_M0, PIN_M1, PIN_AUX);


BLECharacteristic *newMessageCharacteristic;


class ServerCallbacks : public BLEServerCallbacks {
    void onDisconnect(BLEServer* bleServer) {
        SerialDebug.println("BLE device disconnected");
        bleServer->startAdvertising();
    }
};

void setup()
{
    Serial.begin(9600);
    delay(500);
    Serial.println();

    SerialDebug.println("Initializing BLE");
    BLEDevice::init(BLE_DEVICE_NAME);
    BLEServer *bleServer = BLEDevice::createServer();
    bleServer->setCallbacks(new ServerCallbacks());
    BLEService *bleService = bleServer->createService(SERVICE_UUID);
    // TODO: Using notify for now, but might switch to indicate for confirming message
    newMessageCharacteristic = bleService->createCharacteristic(
                                            NEW_MESSAGE_CHARACTERISTIC_UUID,
                                            BLECharacteristic::PROPERTY_READ |
                                            BLECharacteristic::PROPERTY_NOTIFY
                                        );
    // pNewMessageCharacteristic = pService->createCharacteristic(
    //                                         NEW_MESSAGE_CHARACTERISTIC_UUID,
    //                                         BLECharacteristic::PROPERTY_READ
    //                                     );

    // TODO: Might remove
    bleService->start();
    BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
    pAdvertising->addServiceUUID(SERVICE_UUID);
    pAdvertising->setScanResponse(true);
    pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
    pAdvertising->setMinPreferred(0x12);
    BLEDevice::startAdvertising();

    SerialDebug.println("BLE Initialized");

    pinMode(PIN_LED, OUTPUT);

    // Serial2.begin(9600);
    // SerialDebug.println("Lora Module Started");

    // Transceiver.init();

    // Transceiver.SetTransmitRSSI(1);
    // // Transceiver.SetTransmissionMode(0);
    // Transceiver.SetUARTBaudRate(0x3);
    // Transceiver.SetAirDataRate(0x2);
    // Transceiver.SetAddressL(0x0);
    // Transceiver.SetAddressH(0x0);
    // Transceiver.SetListenBeforeTransmit(0x1);
    // // Transceiver.SetChannel(0x0);
    // Transceiver.SetChannel(0x3C);
    // Transceiver.SaveParameters();

    // Transceiver.PrintParameters();

    // ledOnUntil = millis();
    // Last = millis();
}

void loop()
{

    delay(2000);

    // newMessageCharacteristic->setValue("message123");
    // newMessageCharacteristic->notify();

    return;

    if (blinkEnabled) {
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
    }

    if (Serial2.available())
    {
        DATA dataRecv;
        Transceiver.GetStruct(&dataRecv, sizeof(dataRecv));

        char rssi[1];
        Serial2.readBytes(rssi, 1);

        String message = dataRecv.Message + " | RSSI: "+ rssi[0] + "\n";

        newMessageCharacteristic->setValue("message123");
        newMessageCharacteristic->notify();
        //SerialBT.printf("%s | RSSI: %d\n", dataRecv.Message, rssi[0]);

        ledOnUntil = millis() + 100;
    }

    // if (SerialBT.available()) {
    //     DATA dataSend;
    //     dataSend.Message = SerialBT.readString();
    //     Transceiver.SendStruct(&dataSend, sizeof(dataSend));
    // }




    // else if ((millis() - Last) > 5000)
    // {
    //     SerialDebug.println("No response in the last 10 seconds... Sending initial message");
    //     DATA dataSend;
    //     dataSend.Message = "hello!";
    //     dataSend.count = 0;
    //     Transceiver.SendStruct(&dataSend, sizeof(dataSend));

    //     Last = millis();
    // }
}