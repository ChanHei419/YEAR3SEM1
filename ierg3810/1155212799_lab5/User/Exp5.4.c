#include "stm32f10x.h"
//LED active-low
#define DS0_on (GPIOB->BSRR = 1 << (5 + 16)) 
#define DS0_off (GPIOB->BSRR = 1 << 5)
//Exp-5.4
void ds0_turnOff(void)
{GPIOB->BSRR = 1 << 5; //turn off DS0
}
//5.4
// level-two subroutine
void ds0_turnOff2(void)
{ds0_turnOff(); // call level one subroutine
}
void clocktree_init(void) {
//72MHz
}
// DS0 (PB5)
void io_init(void) {
// GPIOB
RCC->APB2ENR |= (1 << 3);
// PB5
GPIOB->CRL &= 0xFF0FFFFF;
// PB5  50MHz
GPIOB->CRL |= 0x00300000;
// DS0
DS0_off;}
// Figure 5.2
void tim3_init_interrupt(u16 arr, u16 psc)
{
// TIM3
RCC->APB1ENR |= 1 << 1;
// Auto-Reload Register (ARR)
TIM3->ARR = arr;
// Prescaler (PSC)
TIM3->PSC = psc;
//Update Interrupt Enable (UIE)
TIM3->DIER |= 1 << 0;
TIM3->CR1 |= 0x01;
// NVIC TIM3(IRQ#29)
NVIC->ISER[0] |= (1 << 29);}
// Exp-5.4
void TIM3_IRQHandler(void)
{DS0_on;             
DS0_off;           
DS0_on;
ds0_turnOff();      //subroutine
DS0_on;
ds0_turnOff2();     //subroutine calls subroutine
DS0_on;           
TIM3->SR &= ~(1 << 0); //RM0008 v21 P410
}
int main(void)
{clocktree_init();
io_init(); //LEDs, Keys and Buzzer
// 72,000,000Hz / (7199+1) / (4999+1) = 2Hz,500ms
tim3_init_interrupt(4999, 7199);
while(1)
{}}