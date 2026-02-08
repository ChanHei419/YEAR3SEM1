#include "stm32f10x.h"
#include "IERG3810_TFTLCD.h"
int main(void)
{char cuid1[] = "1155212799";
char cuid2[] = "1155213082";
lcd_init();
delay_ms(50);
lcd_fillRectangle(0x01E0, 0, LCD_WIDTH, 0, LCD_HEIGHT);
lcd_showString(10, 100, cuid1, WHITE, 0x01E0);
lcd_showString(10, 130, cuid2, LIGHTBLUE, 0x01E0);
while(1)
{}}