#include "IERG3810_KEY.h"
#include "stm32f10x.h"

void IERG3810_KEY_Init(void)
{
    RCC->APB2ENR |= (1 << 2) | (1 << 6);
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

u8 KEY2_Read(void) {
    if (GPIOE->IDR & (1 << 2)) return 1;
    return 0;
}

u8 KEY1_Read(void) {
    if (GPIOE->IDR & (1 << 3)) return 1;
    return 0;
}

u8 KEYUP_Read(void) {
    if (GPIOA->IDR & (1 << 0)) return 1;
    return 0;
}
