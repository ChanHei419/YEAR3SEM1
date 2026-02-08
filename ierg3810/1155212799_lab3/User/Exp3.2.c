#include "stm32f10x.h"
#include "IERG3810_TFTLCD.h"
void Exp3_2_DrawRectangle(void) {
u16 rect_size = 50;
u16 start_x = 0;
u16 start_y = 0;
lcd_fillRectangle(YELLOW, 0, LCD_WIDTH, 0, LCD_HEIGHT);
start_x = (LCD_WIDTH - rect_size) / 2;
start_y = (LCD_HEIGHT - rect_size) / 2;
lcd_fillRectangle(BLUE, start_x, rect_size, start_y, rect_size);
}
int main(void) {
lcd_init();
Exp3_2_DrawRectangle();
while(1) {
}
}