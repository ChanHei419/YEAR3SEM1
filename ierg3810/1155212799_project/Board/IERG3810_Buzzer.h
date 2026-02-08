#ifndef __IERG3810_BUZZER_H
#define __IERG3810_BUZZER_H
#include "stm32f10x.h"

void IERG3810_Buzzer_Init(void);
void Buzzer_On(void);
void Buzzer_Off(void);
void Buzzer_Toggle(void);

#endif
