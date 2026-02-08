#include "JoyPad.h"

static void JoyPad_Delay(volatile uint32_t count)
{
    while(count--);
}

void JoyPad_Init(void)
{
    RCC->APB2ENR |= 1 << 3;

    GPIOB->CRH &= 0xFFF000FF;
    GPIOB->CRH |= 0x00033800;
    
    GPIOB->ODR |= (1 << 10);
    GPIOB->ODR &= ~((1 << 11) | (1 << 12));
}

uint8_t JoyPad_Read(void)
{
    uint8_t temp = 0;
    uint8_t i;

    GPIOB->ODR |= (1 << 11);
    JoyPad_Delay(200);
    GPIOB->ODR &= ~(1 << 11);
    JoyPad_Delay(200);

    for(i = 0; i < 8; i++)
    {
        if((GPIOB->IDR & (1 << 10)) == 0)
        {
            temp |= (1 << i);
        }

        GPIOB->ODR |= (1 << 12);
        JoyPad_Delay(200);
        GPIOB->ODR &= ~(1 << 12);
        JoyPad_Delay(200);
    }

    return temp;
}
