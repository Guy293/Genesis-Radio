#include <Arduino.h>
#include <array>

typedef uint8_t address[4];

class Packet {
public:
    Packet(uint8_t receiver_addr[], uint8_t sender_addr[], String message);
    Packet(uint8_t *data, int length);
    int byte_array_length();
    void to_byte_array(uint8_t *buf);
    uint8_t receiver_addr[4];
    uint8_t sender_addr[4];
    String message;
};