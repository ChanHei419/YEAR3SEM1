#include "stm32f10x.h"
#include "IERG3810_TFTLCD.h"
void Exp3_3_Countdown(void) {
u16 width = 75;
u16 height = 140;
u16 start_x, start_y;
u16 bgcolor = BLACK;
u16 digit_color = GREEN;
s8 i;
lcd_fillRectangle(bgcolor, 0, LCD_WIDTH, 0, LCD_HEIGHT);
start_x = (LCD_WIDTH - width) / 2;
start_y = (LCD_HEIGHT - height) / 2;
while (1) {
for (i = 9; i >= 0; i--) {
lcd_sevenSegment(digit_color, start_x, start_y, i);
delay_ms(1000);
lcd_sevenSegment(bgcolor, start_x, start_y, i);
}}}
int main(void){
lcd_init();
Exp3_3_Countdown();
while(1) {
}}