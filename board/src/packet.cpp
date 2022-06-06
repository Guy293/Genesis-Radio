#include <Arduino.h>
#include <iostream>
#include <string.h>
#include <iterator>
#include <array>
#include "packet.h"
#include "esp_now.h"

using namespace std;

// uint8_t receiver_addr[4];
// uint8_t sender_addr[4];
// uint8_t message[400];
uint8_t receiver_addr[4];
uint8_t sender_addr[4];
String message;

// Packet(byte *sender_addr, byte *receiver_addr, byte *message);
Packet::Packet(uint8_t receiver_addr[], uint8_t sender_addr[], String message) {
    memcpy(this->receiver_addr, receiver_addr, sizeof(4));
    memcpy(this->sender_addr, sender_addr, sizeof(4));
    if (message.length() > ESP_NOW_MAX_DATA_LEN-9) {
        message = message.substring(0, ESP_NOW_MAX_DATA_LEN-9);
    }
    this->message = message;

    // std::copy(receiver_addr, receiver_addr + 4, this->receiver_addr);
    // std::copy(sender_addr, sender_addr + 4, this->sender_addr);
    // this->message = message;
};

// Packet::Packet(uint8_t data[], int length) {
// Packet::Packet(uint8_t data[], int length, int a) {
Packet::Packet(uint8_t data[], int length) {
    int index = 0;

    // 4 bytes of data to receiver_addr
    for (int i = 0; i < 4; i++) {
        receiver_addr[i] = data[index++];
    }

    // 4 bytes of data to sender_addr
    for (int i = 0; i < 4; i++) {
        sender_addr[i] = data[index++];
    }

    // rest of the data to message
    for (int i = 0; i < length-8; i++){
        if (data[index] == '\0') {
            break;
        }
        message.concat((char)data[index++]);
    }

    if (index-1 > ESP_NOW_MAX_DATA_LEN) {
        throw runtime_error("Size of data is too large");
    }
}

int Packet::byte_array_length() {
    // receiver addr + sender addr + message + null byte
    return 4 + 4 + message.length() + 1;
}

void Packet::to_byte_array(uint8_t *buf) {
    int length = byte_array_length();
    uint8_t data[length];

    int index = 0;

    // 4 bytes of data to receiver_addr
    for (int i = 0; i < 4; i++) {
        data[index++] = receiver_addr[i];
    }

    // 4 bytes of data to sender_addr
    for (int i = 0; i < 4; i++) {
        data[index++] = sender_addr[i];
    }

    // Rest of the data to message
    for (int i = 0; i < message.length(); i++){
        data[index++] = message[i];
    }

    // Append null byte
    data[index++] = '\0';

    memcpy(buf, data, length);
}