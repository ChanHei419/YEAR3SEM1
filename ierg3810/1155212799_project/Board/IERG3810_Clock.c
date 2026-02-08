#include "IERG3810_Clock.h"

void clocktree_init(void)
{
    u8 PLL=7;
    unsigned char temp = 0;
    RCC->CR |= 0x00010000;
    while(!(RCC->CR>>17));
    RCC->CFGR = 0x00000400;
    RCC->CFGR &= ~(7 << 11);
    RCC->CFGR |= PLL << 18;
    RCC->CFGR |= 1 << 16;
    FLASH->ACR |= 0x32;
    RCC->CR |= 0x01000000;
    while(!(RCC->CR>>25));
    RCC->CFGR |= 0x00000002;
    while(temp != 0x02)
    {
        temp = RCC->CFGR >> 2;
        temp &= 0x03;
    }
}
