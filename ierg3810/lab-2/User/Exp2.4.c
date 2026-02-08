#include "stm32f10x.h"
#include "IERG3810_clock.h"
#include "IERG3810_USART.h"
void simple_delay(vu32 count);
int main(void)
{char my_sid[] = "1155212799\r\n";
char message[] = "Hello!\r\n";
clocktree_init();
usart1_init(72, 9600);
usart2_init(36, 9600);
simple_delay(7200000);
usart_print_txe(1, my_sid);
while(1)
{usart_print_txe(2, message);
simple_delay(7200000);}}
void simple_delay(vu32 count)
{for(; count != 0; count--);}
