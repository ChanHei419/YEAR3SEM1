#include "IERG3810_USART.h"
#include "stm32f10x.h" 

void delay(u32 count)
{
    u32 i;
    for(i = 0; i < count; i++);
}

void usart2_init(u32 pclk1, u32 baud)
{
    float temp;
    u16 mantissa;
    u16 fraction;
    temp = (float)(pclk1 * 1000000) / (baud * 16);
    mantissa = temp;
    fraction = (temp - mantissa) * 16;
    mantissa <<= 4;
    mantissa += fraction;
    RCC->APB2ENR |= 1 << 2;
    RCC->APB1ENR |= 1 << 17;
    GPIOA->CRL &= 0xFFFF00FF;
    GPIOA->CRL |= 0x00008B00;
    RCC->APB1RSTR |= 1 << 17;
    RCC->APB1RSTR &= ~(1 << 17);
    USART2->BRR = mantissa;
    USART2->CR1 |= 0x2008;
}

void usart1_init(u32 pclk2, u32 baud)
{
    float temp;
    u16 mantissa;
    u16 fraction;
    temp = (float)(pclk2 * 1000000) / (baud * 16);
    mantissa = temp;
    fraction = (temp - mantissa) * 16;
    mantissa <<= 4;
    mantissa += fraction;
    RCC->APB2ENR |= 1 << 14;
    RCC->APB2ENR |= 1 << 2;
    GPIOA->CRH &= 0xFFFFF00F;
    GPIOA->CRH |= 0x000008B0;
    RCC->APB2RSTR |= 1 << 14;
    RCC->APB2RSTR &= ~(1 << 14);
    USART1->BRR = mantissa;
    USART1->CR1 |= 0x2008;
}

void usart_print_txe(u8 USARTport, char *st)
{
    u8 i = 0;
    while (st[i] != 0)
    {
      if (USARTport == 1)
      {
        while ((USART1->SR & 0x80) == 0);
        USART1->DR = st[i];
      } 
      if (USARTport == 2)
      {
        while ((USART2->SR & 0x80) == 0);
        USART2->DR = st[i];
      }    
      if (i == 255) break;    
      i++;
    }
}
