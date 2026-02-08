#include "stm32f10x.h"
void nvic_setPriorityGroup(u8 priGroup) {  }
void clocktree_init(void) {  }
void io_init(void) {
RCC->APB2ENR |= (1 << 3);
GPIOB->CRL &= 0xFF0FFFFF;
GPIOB->CRL |= 0x00300000;
GPIOB->BSRR = (1 << 5); // DS0_off
}
void tim3_init(u16 arr, u16 psc) {
RCC->APB1ENR |= 1 << 1;
TIM3->ARR = arr;
TIM3->PSC = psc;
TIM3->DIER |= 1 << 0;
TIM3->CR1 |= 0x01;
NVIC->IP[29] = 0x45;
NVIC->ISER[0] |= (1 << 29);
}
void TIM3_IRQHandler(void) {
GPIOB->BRR = 1 << 5;
GPIOB->BSRR = 1 << 5;
GPIOB->BRR = 1 << 5;
GPIOB->BSRR = 1 << 5;
GPIOB->ODR ^= 1 << 5;
GPIOB->ODR ^= 1 << 5;
GPIOB->ODR ^= 1 << 5;
GPIOB->ODR ^= 1 << 5;
GPIOB->ODR &= ~(1 << 5);
GPIOB->ODR |= 1 << 5;
GPIOB->ODR &= ~(1 << 5);
GPIOB->ODR |= 1 << 5;
TIM3->SR &= ~(1 << 0);
TIM3->SR &= ~(1 << 0);
}

int main(void) {
io_init();
clocktree_init();
nvic_setPriorityGroup(5);
tim3_init(499, 7999); // 0.5s
while (1) { ; }
}