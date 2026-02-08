#include "stm32f10x.h"

void Delay(u32 count)
{u32 i;
for(i=0; i<count; i++);}

int main(void)
{
RCC->APB2ENR |= 1 << 3;
GPIOB->CRL |= (0x3 << 20);
while(1)
{
GPIOB->BRR = 1 << 5;
Delay(3000000);
GPIOB->BSRR = 1 << 5;
Delay(3000000);
}}


