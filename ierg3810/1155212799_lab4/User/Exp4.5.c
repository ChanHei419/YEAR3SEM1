#include "stm32f10x.h"
#define DS0_on      (GPIOB->BSRR = (1 << (5 + 16)))
#define DS0_off     (GPIOB->BSRR = (1 << 5))
#define DS1_on      (GPIOE->BSRR = (1 << (5 + 16)))
#define DS1_off     (GPIOE->BSRR = (1 << 5))
volatile u32 sheep = 0;
volatile u32 ps2_clk_count = 0;
volatile u32 ps2_data = 0;
volatile u8 ps2_data_ready = 0;
volatile u8 ps2_key_code = 0;
volatile u8 ps2_release_flag = 0;
void delay(u32 count)
{volatile u32 i;
for(i = 0;i < count;i++);}
void ps2_init(void)
{RCC->APB2ENR |= 1<<4;
RCC->APB2ENR |= 0x01;
GPIOC->CRH &= 0xFFFF00FF;
GPIOC->CRH |= 0x00008800;
GPIOC->ODR |= (1 << 10) | (1 << 11);
AFIO->EXTICR[2] &= 0xFFFF0FFF;
AFIO->EXTICR[2] |= 0x00002000;
EXTI->IMR |= 1 << 11;
EXTI->FTSR |= 1 << 11;
NVIC->IP[40] = 0x22;
NVIC->ISER[1] |= (1 << (40 - 32));}
void nvic_setPriorityGroup(u8 priGroup)
{u32 tmp1, tmp2;
tmp2 = (priGroup & 0x00000007) << 8;
tmp1 = SCB->AIRCR & 0x0000F8FF;
tmp1 |= 0x05FA0000;
SCB->AIRCR = tmp1 | tmp2;}
void EXTI15_10_IRQHandler(void)
{if (EXTI->PR & (1 << 11)){
if (ps2_clk_count == 0){
ps2_data = 0;
ps2_clk_count = 1;}
else if (ps2_clk_count < 9)
{if (GPIOC->IDR & (1 << 10)){
ps2_data |= (1 << (ps2_clk_count - 1));}
ps2_clk_count++;}
else if (ps2_clk_count == 9){
ps2_clk_count++;}
else if (ps2_clk_count == 10)
{ps2_key_code = (u8)ps2_data;
ps2_data_ready = 1;
ps2_clk_count = 0;}
EXTI->PR = 1 << 11;}}
void io_init(void) {
RCC->APB2ENR |= (1 << 3);
RCC->APB2ENR |= (1 << 6);
GPIOB->CRL &= 0xFF0FFFFF;
GPIOB->CRL |= 0x00300000;
DS0_off;
GPIOE->CRL &= 0xFF0FFFFF;
GPIOE->CRL |= 0x00300000;
DS1_off;}
void clocktree_init(void) {}
int main(void)
{io_init();
clocktree_init();
nvic_setPriorityGroup(5);
ps2_init();
while(1)
{if (ps2_data_ready){
if (ps2_key_code == 0xF0){
ps2_release_flag = 1;}
else if (ps2_release_flag == 0){
if (ps2_key_code == 0x69){
GPIOB->ODR ^= (1 << 5);}
else if (ps2_key_code == 0x72){
GPIOE->ODR ^= (1 << 5);}}
else{
ps2_release_flag = 0;}
ps2_data_ready = 0;}}
}