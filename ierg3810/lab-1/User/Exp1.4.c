#include "stm32f10x.h"

void Delay(vu32 count)
{for(; count != 0; count--);
}

int main(void)
{RCC->APB2ENR |= (1 << 2) | (1 << 3) | (1 << 6);
GPIOB->CRL &= 0xFF0FFFFF;
GPIOB->CRL |= 0x00300000;
GPIOE->CRL &= 0xFF0FFFFF;
GPIOE->CRL |= 0x00300000;
GPIOB->CRH &= 0xFFFFFFF0;
GPIOB->CRH |= 0x00000003;
GPIOA->CRL &= 0xFFFFFFF0;
GPIOA->CRL |= 0x00000008;
GPIOA->ODR &= ~(1 << 0);
GPIOE->CRL &= 0xFFFFF0FF;
GPIOE->CRL |= 0x00000800;
GPIOE->ODR |= (1 << 2);
GPIOE->CRL &= 0xFFFF0FFF;
GPIOE->CRL |= 0x00008000;
GPIOE->ODR |= (1 << 3);
GPIOB->BSRR = 1 << 5;
GPIOE->BSRR = 1 << 5;
GPIOB->BRR = 1 << 8;
while (1)
{if( !(GPIOE->IDR & (1 << 2)) )
{
GPIOB->BRR = 1 << 5;
}
else
{
GPIOB->BSRR = 1 << 5;
}
if(!(GPIOE->IDR & (1 << 3)))
{Delay(10000);
if(!(GPIOE->IDR & (1 << 3)))
{
if(GPIOE->ODR & (1 << 5))
{
GPIOE->BRR = 1 << 5;
}
else
{
GPIOE->BSRR = 1 << 5;
}
while( !(GPIOE->IDR & (1 << 3)) );
}}
if (GPIOA->IDR & (1 << 0))
{Delay(10000);
if (GPIOA->IDR & (1 << 0))
{
if (GPIOB->ODR & (1 << 8))
{
GPIOB->BRR = 1 << 8;
}
else{
GPIOB->BSRR = 1 << 8;
}
while(GPIOA->IDR & (1 << 0));
}}}}
