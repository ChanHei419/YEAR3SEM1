#include "stm32f10x.h"
#include "IERG3810_KEY.h"
#include "stm32f10x.h"
void IERG3810_KEY_Init(void)
{RCC->APB2ENR|=(1<<2)|(1<<6);
GPIOA->CRL &= 0xFFFFFFF0;
GPIOA->CRL |= 0x00000008;
GPIOA->ODR &= ~(1 << 0);
GPIOE->CRL &= 0xFFFFF0FF; 
GPIOE->CRL |= 0x00000800; 
GPIOE->ODR |= (1 << 2);
GPIOE->CRL &= 0xFFFF0FFF; 
GPIOE->CRL |= 0x00008000; 
GPIOE->ODR |= (1 << 3);
}


