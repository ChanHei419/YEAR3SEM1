#include "IERG3810_io.h"
#include "IERG3810_LED.h"
#include "IERG3810_KEY.h"
#include "IERG3810_Buzzer.h"

void io_init(void) {
    IERG3810_LED_Init();
    IERG3810_KEY_Init();
    IERG3810_Buzzer_Init();
}
