#include <stm32f10x.h>
void clocktree_init(void);
void usart2_init(u32 pclk1, u32 baud);
void delay(vu32 count);
int main(void)
{clocktree_init();
usart2_init(36, 9600);
delay(2000000);
while(1){
USART2->DR = 0x41;
delay(500000);
USART2->DR = 0x42;
delay(50000);
delay(500000);}}
void clocktree_init(void){
unsigned char temp = 0;
RCC->CR |= 1 << 16; 
while (!((RCC->CR >> 17) & 1)); 
FLASH->ACR |= (1 << 5) | (1 << 4) | (1 << 1); 
RCC->CFGR &= ~(0xF << 4); 
RCC->CFGR &= ~(0x7 << 11); 
RCC->CFGR |= (0x4 << 8);
RCC->CFGR |= (1 << 16); 
RCC->CFGR |= (0x7 << 18);
RCC->CR |= (1 << 24); 
while (!((RCC->CR >> 25) & 1));
RCC->CFGR |= (0x2 << 0); 
while (temp != 0x02){
temp = (RCC->CFGR >> 2) & 0x03;}}
void usart2_init(u32 pclk1, u32 baud){
float temp;
u16 mantissa;
u16 fraction;
temp = (float)(pclk1 * 1000000) / (baud * 16);
mantissa = temp;
fraction = (temp - mantissa) * 16; 
RCC->APB2ENR |= 1 << 2;  
RCC->APB1ENR |= 1 << 17;
GPIOA->CRL &= 0xFFFF00FF; 
GPIOA->CRL |= 0x00008B00;
RCC->APB1RSTR |= 1 << 17;
RCC->APB1RSTR &= ~(1 << 17);
USART2->BRR = (mantissa << 4) + fraction;
USART2->CR1 |= (1 << 13) | (1 << 3);}
void delay(vu32 count){ 
vu32 z;
for(z=0; z<count; z++);}
