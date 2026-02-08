#ifndef __JOYPAD_H
#define __JOYPAD_H

#include "stm32f10x.h"

#define JOYPAD_KEY_A        0x01
#define JOYPAD_KEY_B        0x02
#define JOYPAD_KEY_SELECT   0x04
#define JOYPAD_KEY_START    0x08
#define JOYPAD_KEY_UP       0x10
#define JOYPAD_KEY_DOWN     0x20
#define JOYPAD_KEY_LEFT     0x40
#define JOYPAD_KEY_RIGHT    0x80

void JoyPad_Init(void);
uint8_t JoyPad_Read(void);

#endif
