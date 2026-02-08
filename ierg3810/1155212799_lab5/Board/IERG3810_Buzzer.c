#include "IERG3810_Buzzer.h"
#include "stm32f10x.h"
void IERG3810_Buzzer_Init(void)
{
RCC->APB2ENR |= (1 << 3);
GPIOB->CRH &= 0xFFFFFFF0; 
GPIOB->CRH |= 0x00000003; 
GPIOB->BRR = 1 << 8;
}
void Buzzer_On(void) {
GPIOB->BSRR = 1 << 8;
}
void Buzzer_Off(void) {
GPIOB->BRR = 1 << 8;
}
void Buzzer_Toggle(void) {
if(GPIOB->ODR & (1<<8))
{
GPIOB->BRR = 1 << 8;}
else
{
GPIOB->BSRR = 1 << 8;
}}





