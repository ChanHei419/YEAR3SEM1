#include "IERG3810_TFTLCD.h"
#include "stm32f10x.h"
int main(void) {
char cuid1[] = "1155212799";
char cuid2[] = "1155213082";
u16 bg_color = BLACK;
lcd_init();
lcd_fillRectangle(bg_color, 0, LCD_WIDTH, 0, LCD_HEIGHT);
delay_ms(50);
lcd_showString(10, 100, cuid1, YELLOW, bg_color);
lcd_showChinChar(10 + 8 * 11, 100, 0, YELLOW, bg_color);
lcd_showChinChar(10 + 8 * 11 + 16, 100, 1, YELLOW, bg_color); 
lcd_showChinChar(10 + 8 * 11 + 32, 100, 2, YELLOW, bg_color); 
lcd_showString(10, 130, cuid2, CYAN, bg_color);
lcd_showChinChar(10 + 8 * 11, 130, 3, CYAN, bg_color);
lcd_showChinChar(10 + 8 * 11 + 16, 130, 4, CYAN, bg_color);
lcd_showChinChar(10 + 8 * 11 + 32, 130, 5, CYAN, bg_color); 
while (1) {}}