#include "stm32f10x.h"
//Figure 5.11
#define LED0_PWM_VAL TIM3->CCR2  
#define DS1_off     (GPIOE->BSRR = (1 << 5))
volatile u32 heartbeat[10] = {0};
void clocktree_init(void) {
//72MHz}
extern volatile u32 heartbeat[10];
void TIM4_IRQHandler(void) {
if (TIM4->SR & 1 << 0) {
// DS1 (PE5)
GPIOE->ODR ^= 1 << 5;}
TIM4->SR &= ~(1 << 0);}
void io_init(void) {
// DS1 (PE5)
RCC->APB2ENR |= (1 << 6); // PIOE
GPIOE->CRL &= 0xFF0FFFFF; // PE5
GPIOE->CRL |= 0x00300000; // PE5
DS1_off; // ????? DS1}
void tim3_init_pwm(u16 arr, u16 psc)
{
RCC->APB2ENR |= 1 << 3;       //RM0008 v21 P112
GPIOB->CRL &= 0xFF0FFFFF;
GPIOB->CRL |= 0x00B00000;     //RM0008 v21 P171
RCC->APB2ENR |= 1 << 0;
AFIO->MAPR &= 0xFFFFF3FF;     //RM0008 v21 P184
AFIO->MAPR |= 1 << 11;        //RM0008 v21 P184
RCC->APB1ENR |= 1 << 1;       //RM0008 v21 P115
TIM3->ARR = arr;              //RM0008 v21 P419
TIM3->PSC = psc;              //RM0008 v21 P418
TIM3->CCMR1 |= 7<<12;         //RM0008 v21 P413
TIM3->CCMR1 |= 1<<11;         //RM0008 v21 P413
TIM3->CCER |= 1<<4;           //RM0008 v21 P417
TIM3->CR1 |= 0x0080;          //RM0008 v21 P404
TIM3->CR1 |= 0x01;            //RM0008 v21 P404
}
void systick_init_10ms(void)
{
//SYSTICK
SysTick->CTRL = 0; // clear
SysTick->LOAD = 89999; // What should be filled? Refer to DDI-0337E
// CLKSOURCE=0: STCLK (FCLK/8)
// CLKSOURCE=1: FCLK/1
// CLKSOURCE=0 is synchronized and better than CLKSOURCE=1
// set Clock tree on RM0008 pag-93
SysTick->CTRL |= 0x03; // What should be filled?
// set internal clock, use interrupt, start count
}
void tim4_init(u16 arr, u16 psc) {
RCC->APB1ENR |= 1 << 2; // Enable TIM4 clock
TIM4->ARR = arr;
TIM4->PSC = psc;
TIM4->DIER |= 1 << 0; // Enable update interrupt
TIM4->CR1 |= 0x01;    // Enable counter

// Enable TIM4 interrupt in NVIC (IRQ#30)
NVIC->ISER[0] |= (1 << 30);
}
int main(void)
{
u16 led0pwmval = 0; 
u8 dir = 1;         

clocktree_init();
io_init(); //LEDs, Keys and Buzzer
systick_init_10ms();
// 150Hz
// F_pwm = 72MHz / (PSC+1) / (ARR+1) = 150Hz
// (PSC+1) * (ARR+1) = 480,000
// ARR=9999, PSC=47
tim3_init_pwm(9999, 47);
// 4b: DS1 flash with Timer-4 as Exp-5.2
// 4Hz -> 250ms -> 125ms
// F_int = 72MHz / (PSC+1) / (ARR+1) = 1 / 0.125s = 8Hz
// (PSC+1) * (ARR+1) = 9,000,000
// PSC=7199, ? ARR=1249
tim4_init(1249, 7199);
// pWM 
// 1=10ms  PWM
heartbeat[0] = 1;
while(1)
{//Figure 5.7
if (heartbeat[0] == 1)
{heartbeat[0] = 1; 
//Figure 5.12
if (dir) led0pwmval++;
else led0pwmval--;
if (led0pwmval > 5000) dir=0;
if (led0pwmval == 0) dir=1;
LED0_PWM_VAL = led0pwmval;}}}