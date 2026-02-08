#include "stm32f10x.h"
#define DS0_on      (GPIOB->BSRR = (1 << (5 + 16)))
#define DS0_off     (GPIOB->BSRR = (1 << 5))
#define DS1_on      (GPIOE->BSRR = (1 << (5 + 16)))
#define DS1_off     (GPIOE->BSRR = (1 << 5))
volatile u32 sheep = 0;
void delay(u32 count)
{volatile u32 i;
for(i = 0;i < count;i++);}
void key2_extiInit(void)
{RCC->APB2ENR |= 1<<6;
GPIOE->CRL &= 0XFFFFF0FF;
GPIOE->CRL |= 0X00000800;
GPIOE->ODR |= 1 << 2;
RCC->APB2ENR |= 0x01;
AFIO->EXTICR[0] &= 0xFFFFF0FF;
AFIO->EXTICR[0] |= 0x00000400;
EXTI->IMR |= 1<<2;
EXTI->FTSR |= 1<<2;
NVIC->IP[8] = 0x65;
NVIC->ISER[0] |= (1<<8);}
void keyup_extiInit(void)
{RCC->APB2ENR |= 1<<2;
GPIOA->CRL &= 0xFFFFFFF0;
GPIOA->CRL |= 0x00000008;
GPIOA->ODR &= ~(1 << 0);
RCC->APB2ENR |= 0x01;
AFIO->EXTICR[0] &= 0xFFFFFFF0;
AFIO->EXTICR[0] |= 0x00000000;
EXTI->IMR |= 1<<0;
EXTI->RTSR |= 1<<0;
NVIC->IP[6] = 0x75;
NVIC->ISER[0] |= (1<<6);}
void nvic_setPriorityGroup(u8 priGroup)
{u32 tmp1, tmp2;
tmp2 = (priGroup & 0x00000007) << 8;
tmp1 = SCB->AIRCR & 0x0000F8FF;
tmp1 |= 0x05FA0000;
SCB->AIRCR = tmp1 | tmp2;}
void EXTI0_IRQHandler(void)
{u8 i;
for (i=0; i<10; i++)
{
DS1_on;
delay(1000000);
DS1_off;
delay(1000000);}
EXTI->PR = 1<<0;}
void EXTI2_IRQHandler(void)
{u8 i;
for (i=0; i<10; i++)
{DS0_on;
delay(1000000);
DS0_off;
delay(1000000);}
EXTI->PR = 1<<2;}
void io_init(void) {
RCC->APB2ENR |= (1 << 3);
RCC->APB2ENR |= (1 << 6);
GPIOB->CRL &= 0xFF0FFFFF;
GPIOB->CRL |= 0x00300000;
DS0_off;
GPIOE->CRL &= 0xFF0FFFFF;
GPIOE->CRL |= 0x00300000;
DS1_off;}
void clocktree_init(void) {
}
int main(void)
{
io_init();
clocktree_init();
nvic_setPriorityGroup(5);
key2_extiInit();
keyup_extiInit();

while(1)
{
sheep++;
}
}