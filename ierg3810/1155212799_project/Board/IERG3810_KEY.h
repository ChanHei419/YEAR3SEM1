#ifndef __IERG3810_KEY_H
#define __IERG3810_KEY_H
#include "stm32f10x.h"

void IERG3810_KEY_Init(void);
u8 KEY2_Read(void);
u8 KEY1_Read(void);
u8 KEYUP_Read(void);

#endif
