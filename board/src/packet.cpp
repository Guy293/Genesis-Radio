#include <Arduino.h>
#include <iostream>
#include <string.h>
#include <iterator>
#include <array>
#include "packet.h"

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
    this->message = message;

    // std::copy(receiver_addr, receiver_addr + 4, this->receiver_addr);
    // std::copy(sender_addr, sender_addr + 4, this->sender_addr);
    // this->message = message;
};

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

    // SerialDebug.printf("Length: %d\n", length);
    // SerialDebug.printf("Index: %d\n", index);
    // SerialDebug.printf("Length - (index-1): %d\n", length - (index-1));

    // rest of the data to message
    // for (int i = 0; i < length-(index-1); i++){
    for (int i = 0; i < length-8; i++){
        // SerialDebug.printf("i: %d  %c %x\n", i, data[index], data[index]);
        // SerialDebug.printf("%s\n", message);
        if (data[index] == '\0') {
            // SerialDebug.printf("BREAK\n");
            break;
        }
        message += (char)data[index++];
    }

    if (index-1 > 408) {
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