#include "stm32f10x.h"
#include "IERG3810_TFTLCD.h"

void Exp3_1_DrawLines(void){
u16 colors[] = {BLACK, WHITE, GREEN, RED, BLUE};
u16 x_positions[] = {10, 20, 30, 40, 50};
int i, j;
lcd_fillRectangle(YELLOW, 0, LCD_WIDTH, 0, LCD_HEIGHT);
for (i = 0; i < 5; i++) {
for (j = 0; j < 100; j++) {
lcd_drawDot(x_positions[i], 10 + j, colors[i]);
}
}
}
int main(void) {
lcd_init();
Exp3_1_DrawLines();
while(1) {
}
}