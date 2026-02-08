#include "IERG3810_LED.h"
#include "stm32f10x.h"

void IERG3810_LED_Init(void)
{RCC->APB2ENR |= (1 << 3) | (1 << 6);
GPIOB->CRL &= 0xFF0FFFFF; 
GPIOB->CRL |= 0x00300000; 
GPIOE->CRL &= 0xFF0FFFFF;
GPIOE->CRL |= 0x00300000; 
GPIOB->BSRR = 1 << 5;
GPIOE->BSRR = 1 << 5;
}
void DS0_On(void){
GPIOB->BRR = 1 << 5;
}
void DS0_Off(void){
GPIOB->BSRR = 1 << 5;
}
void DS1_On(void){
GPIOE->BRR = 1 << 5;
}
void DS1_Off(void){
GPIOE->BSRR = 1 << 5;
}

void DS1_Toggle(void){
if(GPIOE->ODR&(1 << 5))
{GPIOE->BRR = 1 << 5;
}
else
{GPIOE->BSRR = 1 << 5;}
}
