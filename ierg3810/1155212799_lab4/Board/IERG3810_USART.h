#ifndef __IERG3810_USART_H
#define __IERG3810_USART_H
#include "stm32f10x.h"
void usart1_init(u32 pclk2, u32 baud);
void usart2_init(u32 pclk1, u32 baud);
void usart_print_txe(u8 USARTport, char *st);
#endif