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
#include "packet.h"

// #include <WiFi.h>
// #include <ESPmDNS.h>
// #include <WiFiUdp.h>
// #include <ArduinoOTA.h>

// #include <ArduinoJson.h>

#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>
#include <WiFi.h>

#define SERVICE_UUID "16f88c52-1471-4bba-95a8-17094b0520d3"
#define NEW_MESSAGE_CHARACTERISTIC_UUID "af77d21b-1a5c-4910-b4b4-c98220ac0e79"
#define SEND_MESSAGE_CHARACTERISTIC_UUID "8ef6e254-8921-4ef2-9726-368055789ba4"

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

#define DEVICE_1 "4C:EB:D6:7C:02:60"
#define DEVICE_2 "58:BF:25:80:F7:FC"

unsigned int ledOnUntil;
bool ledStatus;
String mac;

// create the transceiver object, passing in the serial and pins
EBYTE Transceiver(&Serial2, PIN_M0, PIN_M1, PIN_AUX);


BLECharacteristic *newMessageCharacteristic;
BLECharacteristic *sendMessageCharacteristic;


class ServerCallbacks : public BLEServerCallbacks {
    void onConnect(BLEServer* bleServer) {
        SerialDebug.println("BLE device connected");
        // Start advertising again because the module stops advertising after connection
        bleServer->startAdvertising();
    }

    void onDisconnect(BLEServer* bleServer) {
        SerialDebug.println("BLE device disconnected");
    }
};

class SendMessageCallbacks: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pCharacteristic) {
        std::string message = pCharacteristic->getValue();

        SerialDebug.printf("Sending message: %s\n", message.c_str());

        address receiver_addr = {0, 0, 0, 0};
        address sender_addr = {0, 0, 0, 0};

        Packet p(receiver_addr, sender_addr, message.c_str());

        uint8_t buf[p.byte_array_length()];
        p.to_byte_array(buf);

        Serial2.write(buf, p.byte_array_length());
    }
};

void setup()
{
    Serial.begin(9600);
    delay(500);
    Serial.println();

    mac = WiFi.macAddress();

    if (mac == DEVICE_1) {
        SerialDebug.println("Device 1");
    } else if (mac == DEVICE_2) {
        SerialDebug.println("Device 2");
    } else {
        SerialDebug.println("Unknown device");
    }

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
    sendMessageCharacteristic = bleService->createCharacteristic(
                                            SEND_MESSAGE_CHARACTERISTIC_UUID,
                                            BLECharacteristic::PROPERTY_WRITE
                                        );
    sendMessageCharacteristic->setCallbacks(new SendMessageCallbacks());

    if (mac == DEVICE_2) {
        // TODO: Might remove
        bleService->start();
        BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
        pAdvertising->addServiceUUID(SERVICE_UUID);
        pAdvertising->setScanResponse(true);
        pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
        pAdvertising->setMinPreferred(0x12);
        BLEDevice::startAdvertising();

        SerialDebug.println("BLE Initialized");
    }

    pinMode(PIN_LED, OUTPUT);

    Serial2.begin(9600);

    Transceiver.init();

    Transceiver.SetTransmitRSSI(1);
    // Transceiver.SetTransmissionMode(0);
    Transceiver.SetUARTBaudRate(0x3);
    Transceiver.SetAirDataRate(0x2);
    Transceiver.SetAddressL(0x0);
    Transceiver.SetAddressH(0x0);
    Transceiver.SetListenBeforeTransmit(0x1);
    // Transceiver.SetChannel(0x0);
    Transceiver.SetChannel(0x3C); // Frequency - 850.125 + channel
    Transceiver.SetTransmitPower(0x00);
    Transceiver.SaveParameters();

    SerialDebug.println("Lora Module Initialized");

    // Transceiver.PrintParameters();

    ledOnUntil = 0;
}

void blink_led_loop()
{
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

void loop()
{
    if (blinkEnabled) {
        blink_led_loop();
    }

    // if (mac == DEVICE_1) {
    //     if (SerialDebug.available()) {
    //         String message = SerialDebug.readString();
    //         address receiver_addr = {10, 20, 10, 20};
    //         address sender_addr = {10, 20, 10, 20};

    //         Packet p(receiver_addr, sender_addr, message);

    //         SerialDebug.printf("Sending message: %s\n", message);

    //         uint8_t buf[p.byte_array_length()];
    //         p.to_byte_array(buf);

    //         Serial2.write(buf, p.byte_array_length());

    //         ledOnUntil = millis() + 50;

    //         return;
    //     }
    // }

    if (Transceiver.available())
    {
        uint8_t buf[408];

        int length = Serial2.readBytesUntil('\0', buf, 408);

        SerialDebug.printf("Length: %d\n", length);

        Packet p(buf, length);

        SerialDebug.printf("%d.%d.%d.%d\n", p.receiver_addr[0], p.receiver_addr[1], p.receiver_addr[2], p.receiver_addr[3]);
        SerialDebug.printf("%d.%d.%d.%d\n", p.sender_addr[0], p.sender_addr[1], p.sender_addr[2], p.sender_addr[3]);
        // SerialDebug.println(p.message);

        char rssi[1];
        Serial2.readBytes(rssi, 1);

        SerialDebug.printf("Recieved message (RSSI: %d): %s\n", rssi[0], p.message);

        char message_cArray[p.message.length()];

        p.message.toCharArray(message_cArray, p.message.length());

        newMessageCharacteristic->setValue(message_cArray);
        newMessageCharacteristic->notify();

        ledOnUntil = millis() + 50;
    }
}