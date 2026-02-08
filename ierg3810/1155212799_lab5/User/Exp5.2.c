#include "stm32f10x.h"
#define DS0_on      (GPIOB->BSRR = (1 << (5 + 16)))
#define DS0_off     (GPIOB->BSRR = (1 << 5))
#define DS1_on      (GPIOE->BSRR = (1 << (5 + 16)))
#define DS1_off     (GPIOE->BSRR = (1 << 5))
void nvic_setPriorityGroup(u8 priGroup) {
u32 tmp1, tmp2;
tmp2 = (priGroup & 0x00000007) << 8;
tmp1 = SCB->AIRCR & 0x0000F8FF;
tmp1 |= 0x05FA0000;
SCB->AIRCR = tmp1 | tmp2;}
void clocktree_init(void) {}
void io_init(void) {
RCC->APB2ENR |= (1 << 3) | (1 << 6); // GPIOB  GPIOE
GPIOB->CRL &= 0xFF0FFFFF; 
GPIOB->CRL |= 0x00300000; // PB5 
DS0_off; 
GPIOE->CRL &= 0xFF0FFFFF; 
GPIOE->CRL |= 0x00300000; 
DS1_off; }
void tim3_init(u16 arr, u16 psc) {
RCC->APB1ENR |= 1 << 1;
TIM3->ARR = arr;
TIM3->PSC = psc;
TIM3->DIER |= 1 << 0;
TIM3->CR1 |= 0x01;
NVIC->IP[29] = 0x45;
NVIC->ISER[0] |= (1 << 29);}
void tim4_init(u16 arr, u16 psc) {
RCC->APB1ENR |= 1 << 2;
TIM4->ARR = arr;
TIM4->PSC = psc;
TIM4->DIER |= 1 << 0;
TIM4->CR1 |= 0x01;
NVIC->IP[30] = 0x48;
NVIC->ISER[0] |= (1 << 30);}
void TIM3_IRQHandler(void) {
if (TIM3->SR & 1 << 0) {
GPIOB->ODR ^= 1 << 5;}
TIM3->SR &= ~(1 << 0);}
void TIM4_IRQHandler(void) {
if (TIM4->SR & 1 << 0) {
GPIOE->ODR ^= 1 << 5;}
TIM4->SR &= ~(1 << 0);}
int main(void) {
io_init();
clocktree_init();
nvic_setPriorityGroup(5);
tim3_init(499, 7999);  // 0.5s
tim4_init(124, 7999);  // 0.125s
while (1) {;}}