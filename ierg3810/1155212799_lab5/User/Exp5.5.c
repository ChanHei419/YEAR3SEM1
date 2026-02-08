#include "stm32f10x.h"
#define DS0_on      (GPIOB->BSRR = (1 << (5 + 16)))
#define DS0_off     (GPIOB->BSRR = (1 << 5))
#define DS1_on      (GPIOE->BSRR = (1 << (5 + 16)))
#define DS1_off     (GPIOE->BSRR = (1 << 5))

void clocktree_init(void) {}
void io_init(void) {
RCC->APB2ENR |= (1 << 3) | (1 << 6);
GPIOB->CRL &= 0xFF0FFFFF; GPIOB->CRL |= 0x00300000; DS0_off;
GPIOE->CRL &= 0xFF0FFFFF; GPIOE->CRL |= 0x00300000; DS1_off;
}

//Figure 5.5
void systick_init_10ms(void)
{
SysTick->CTRL = 0;
SysTick->LOAD = 9999; 
SysTick->CTRL = 0x03; 
}
// Figure 5.6 & 5.7
volatile u32 heartbeat[10] = {0};

int main(void)
{
io_init();
clocktree_init();
systick_init_10ms();

//From Figure 5.7
heartbeat[0] = 50;  // DS0:50 * 10ms = 500ms
heartbeat[1] = 100; //DS1:100 * 10ms = 1000ms
while(1)
{
if (heartbeat[0] == 1)
{
heartbeat[0] = 50;
GPIOB->ODR ^= 1<<5; // DS0
}
if (heartbeat[1] == 1)
{
heartbeat[1] = 100;
GPIOE->ODR ^= 1<<5; //DS1
}
}
}