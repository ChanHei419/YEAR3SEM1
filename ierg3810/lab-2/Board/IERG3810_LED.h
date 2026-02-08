#ifndef __IERG3810_LED_H
#define __IERG3810_LED_H
#include "stm32f10x.h"

void IERG3810_LED_Init(void);
void DS0_On(void);
void DS0_Off(void);
void DS1_On(void);
void DS1_Off(void);
void DS1_Toggle(void);

#endif
