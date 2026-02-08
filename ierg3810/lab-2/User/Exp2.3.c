#include <stm32f10x.h>
void clocktree_init(void);
void usart1_init(u32 pclk2, u32 baud);
void usart2_init(u32 pclk1, u32 baud);
void usart_print_txe(u8 USARTport, char *st);
void simple_delay(vu32 count);
int main(void)
{char my_sid[] = "1155212799\r\n";
char message[] = "Hello\r\n";
clocktree_init();
usart1_init(72, 9600);
usart2_init(36, 9600);
simple_delay(7200000);
usart_print_txe(1, my_sid);
while(1)
{usart_print_txe(2, message);
simple_delay(7200000);}}
void clocktree_init(void)
{unsigned char temp = 0;
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
while (temp != 0x02)
{temp = (RCC->CFGR >> 2) & 0x03;}}
void usart1_init(u32 pclk2, u32 baud)
{float temp;
u16 mantissa;
u16 fraction;
temp = (float)(pclk2 * 1000000) / (baud * 16);
mantissa = temp;
fraction = (temp - mantissa) * 16;
RCC->APB2ENR |= 1 << 2;
RCC->APB2ENR |= 1 << 14;
GPIOA->CRH &= 0xFFFFF00F;
GPIOA->CRH |= 0x000008B0;
RCC->APB2RSTR |= 1 << 14;
RCC->APB2RSTR &= ~(1 << 14);
USART1->BRR = (mantissa << 4) + fraction;
USART1->CR1 |= (1 << 13) | (1 << 3);}
void usart2_init(u32 pclk1, u32 baud)
{float temp;
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
void usart_print_txe(u8 USARTport, char *st)
{u8 i = 0;
while (st[i] != 0)
{if (USARTport == 1){
while( !(USART1->SR & (1 << 7)) );
USART1->DR = st[i];}
if (USARTport == 2){
while( !(USART2->SR & (1 << 7)) );
USART2->DR = st[i];}
if (i == 255) break;
i++;}}
void simple_delay(vu32 count){
for(; count != 0; count--);}
