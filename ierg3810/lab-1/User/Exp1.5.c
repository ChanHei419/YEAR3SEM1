#include "stm32f10x.h"
#include "IERG3810_LED.h"
#include "IERG3810_Buzzer.h"
#include "IERG3810_KEY.h"
void Delay(vu32 count)
{for(;count!=0;count--);}
int main(void)
{
IERG3810_LED_Init();
IERG3810_Buzzer_Init();
IERG3810_KEY_Init();
while (1)
{
if ( !(GPIOE->IDR & (1 << 2)) ) 
{DS0_On();}
else
{DS0_Off();}
if(!(GPIOE->IDR & (1 << 3))) 
{Delay(10000);
if(!(GPIOE->IDR & (1 << 3))) 
{	DS1_Toggle();
		while( !(GPIOE->IDR & (1 << 3)) );
}}
if(GPIOA->IDR & (1 << 0))
{Delay(10000);
if(GPIOA->IDR & (1 << 0)) 
{	Buzzer_Toggle();
		while(GPIOA->IDR & (1 << 0));
}}}}
