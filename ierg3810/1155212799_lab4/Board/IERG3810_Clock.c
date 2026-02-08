#include "IERG3810_clock.h"
void clocktree_init(void)
{unsigned char temp = 0;
RCC->CR |= 1 << 16;
while (!((RCC->CR >> 17) & 1));
FLASH->ACR |= (1 << 5) | (1 << 4) | (1 << 1);
RCC->CFGR &= ~(0xF << 4);
RCC->CFGR &= ~(0x7 << 11);
RCC->CFGR |= (0x4 << 8);
RCC->CFGR |= (1 << 16);
RCC->CFGR |= (0x7 << 18);
RCC->CR |= (1 << 24);
while (!((RCC->CR >> 25) & 1));
RCC->CFGR |= (0x2 << 0);
while (temp != 0x02)
{temp = (RCC->CFGR >> 2) & 0x03;
}}


