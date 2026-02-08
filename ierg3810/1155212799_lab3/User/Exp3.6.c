#include "stm32f10x.h"
#include "IERG3810_TFTLCD.h"
void ShowCoordinateSystem(void);
int main(void) {
lcd_init();
ShowCoordinateSystem();
while(1) {
}}
void ShowCoordinateSystem(void) {
u16 i;
u16 axis_length = 150; 
u32 trace_delay = 10;    
lcd_drawDot(0, 0, YELLOW);
delay_ms(500); 
	//x
for (i = 1; i <= axis_length; i++) {
lcd_drawDot(i, 0, RED); 
delay_ms(trace_delay);        
}
//y
delay_ms(500); 
for (i = 1; i <= axis_length; i++) {
lcd_drawDot(0, i, GREEN);
delay_ms(trace_delay);   
}
}