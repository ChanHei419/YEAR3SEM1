#include "IERG3810_TFTLCD.h"
#include "FONT.H"
#include "CFONT.H"
static void lcd_wr_reg(u16 regval);
static void lcd_wr_data(u16 data);
static void lcd_9341_setParameter(void);
static void lcd_backlight_init(void);
#define LCD_Light_ON (GPIOB->ODR |= (1 << 0))
void delay_ms(u32 i) {
u32 temp;
for (; i > 0; i--) {
for (temp = 10000; temp > 0; temp--);
}}
void lcd_init(void)
{RCC->AHBENR|=1<<8;
RCC->APB2ENR|=1<<3;
RCC->APB2ENR|=1<<5;
RCC->APB2ENR|=1<<6;
RCC->APB2ENR|=1<<8;
GPIOB->CRL&=0XFFFFFFF0;
GPIOB->CRL|=0X00000003;
GPIOD->CRH&=0X00FFF000;
GPIOD->CRH|=0XBB000BBB;
GPIOD->CRL&=0XFF00FF00;
GPIOD->CRL|=0X00BB00BB;
GPIOE->CRH&=0X00000000;
GPIOE->CRH|=0XBBBBBBBB;
GPIOE->CRL&=0X0FFFFFFF;
GPIOE->CRL|=0XB0000000;
GPIOG->CRH&=0XFFF0FFFF;
GPIOG->CRH|=0X000B0000;
GPIOG->CRL&=0XFFFFFFF0;
GPIOG->CRL|=0X0000000B;
FSMC_Bank1->BTCR[6]=0X00000000;
FSMC_Bank1->BTCR[7]=0X00000000;
FSMC_Bank1E->BWTR[6]=0X00000000;
FSMC_Bank1->BTCR[6]|=1<<12;
FSMC_Bank1->BTCR[6]|=1<<14;
FSMC_Bank1->BTCR[6]|=1<<4;
FSMC_Bank1->BTCR[7]|=0<<28;
FSMC_Bank1->BTCR[7]|=1<<0;
FSMC_Bank1->BTCR[7]|=0XF<<8;
FSMC_Bank1E->BWTR[6]|=0<<28;
FSMC_Bank1E->BWTR[6]|=0<<0;
FSMC_Bank1E->BWTR[6]|=3<<8;
FSMC_Bank1->BTCR[6]|=1<<0;
lcd_9341_setParameter();
lcd_backlight_init();
LCD_Light_ON;}
void lcd_9341_setParameter(void)
{lcd_wr_reg(0X01);
delay_ms(50);
lcd_wr_reg(0X11);
delay_ms(120);
lcd_wr_reg(0X3A);
lcd_wr_data(0X55);
lcd_wr_reg(0X36);
lcd_wr_data(0XC8);
lcd_wr_reg(0X29);}
void lcd_wr_reg(u16 regval)
{LCD->LCD_REG=regval;}
void lcd_wr_data(u16 data)
{LCD->LCD_RAM=data;}
static void lcd_backlight_init(void){}
void lcd_drawDot(u16 x, u16 y, u16 color)
{lcd_wr_reg(0x2A);
lcd_wr_data(x>>8);
lcd_wr_data(x & 0xFF);
lcd_wr_data(x>>8);
lcd_wr_data(x & 0xFF);
lcd_wr_reg(0x2B);
lcd_wr_data(y>>8);
lcd_wr_data(y & 0xFF);
lcd_wr_data(y>>8);
lcd_wr_data(y & 0xFF);
lcd_wr_reg(0x2C);
lcd_wr_data(color);}
void lcd_fillRectangle(u16 color, u16 start_x, u16 length_x, u16 start_y, u16 length_y)
{u32 index=0;
u16 end_x = start_x + length_x - 1;
u16 end_y = start_y + length_y - 1;
lcd_wr_reg(0x2A);
lcd_wr_data(start_x>>8);
lcd_wr_data(start_x & 0xFF);
lcd_wr_data(end_x >> 8);
lcd_wr_data(end_x & 0xFF);
lcd_wr_reg(0x2B);
lcd_wr_data(start_y>>8);
lcd_wr_data(start_y & 0xFF);
lcd_wr_data(end_y >> 8);
lcd_wr_data(end_y & 0xFF);
lcd_wr_reg(0x2C);
for(index=0; index < (u32)length_x * length_y; index++)
{lcd_wr_data(color);}}
void lcd_sevenSegment(u16 color, u16 start_x, u16 start_y, u8 digit) {
const u16 thickness = 10;
const u16 horiz_len = 55;
const u16 vert_len = 55;
if (digit == 0 || digit == 2 || digit == 3 || digit == 5 || digit == 6 || digit == 7 || digit == 8 || digit == 9)
lcd_fillRectangle(color, start_x + thickness, horiz_len, start_y + 130, thickness);
if (digit == 0 || digit == 1 || digit == 2 || digit == 3 || digit == 4 || digit == 7 || digit == 8 || digit == 9)
lcd_fillRectangle(color, start_x + 65, thickness, start_y + 75, vert_len);
if (digit == 0 || digit == 1 || digit == 3 || digit == 4 || digit == 5 || digit == 6 || digit == 7 || digit == 8 || digit == 9)
lcd_fillRectangle(color, start_x + 65, thickness, start_y + 10, vert_len);
if (digit == 0 || digit == 2 || digit == 3 || digit == 5 || digit == 6 || digit == 8 || digit == 9)
lcd_fillRectangle(color, start_x + thickness, horiz_len, start_y, thickness);
if (digit == 0 || digit == 2 || digit == 6 || digit == 8)
lcd_fillRectangle(color, start_x, thickness, start_y + 10, vert_len);
if (digit == 0 || digit == 4 || digit == 5 || digit == 6 || digit == 8 || digit == 9)
lcd_fillRectangle(color, start_x, thickness, start_y + 75, vert_len);
if (digit == 2 || digit == 3 || digit == 4 || digit == 5 || digit == 6 || digit == 8 || digit == 9)
lcd_fillRectangle(color, start_x + thickness, horiz_len, start_y + 65, thickness);
}
void lcd_showChar(u16 x, u16 y, u8 ascii, u16 color, u16 bgcolor) {
u8 row, col;
u8 font_data;
u8 font_col, font_row_in_byte;
u16 end_x = x + 8 - 1;
u16 end_y = y + 16 - 1;
if (ascii < 32 || ascii > 126 || x > LCD_WIDTH - 8 || y > LCD_HEIGHT - 16) return;
ascii -= 32;
lcd_wr_reg(0x2A);
lcd_wr_data(x >> 8); lcd_wr_data(x & 0xFF);
lcd_wr_data(end_x >> 8); lcd_wr_data(end_x & 0xFF);
lcd_wr_reg(0x2B);
lcd_wr_data(y >> 8); lcd_wr_data(y & 0xFF);
lcd_wr_data(end_y >> 8); lcd_wr_data(end_y & 0xFF);
lcd_wr_reg(0x2C);
for (row = 0; row < 16; row++) {
for (col = 0; col < 8; col++) {
font_col = 7 - col;
if (row < 8) {
font_data = asc2_1608[ascii][font_col * 2];
font_row_in_byte = 7 - row;
} else {
font_data = asc2_1608[ascii][font_col * 2 + 1];
font_row_in_byte = 7 - (row - 8);
}
if ((font_data >> font_row_in_byte) & 0x01) {
lcd_wr_data(color);
} else {
lcd_wr_data(bgcolor);}}}}
void lcd_showChinChar(u16 x, u16 y, u8 index, u16 color, u16 bgcolor) {
u8 row, bit;
u16 end_x = x + 16 - 1;
u16 end_y = y + 16 - 1;
if (x > LCD_WIDTH - 16 || y > LCD_HEIGHT - 16) return;
lcd_wr_reg(0x2A);
lcd_wr_data(x >> 8); lcd_wr_data(x & 0xFF);
lcd_wr_data(end_x >> 8); lcd_wr_data(end_x & 0xFF);
lcd_wr_reg(0x2B);
lcd_wr_data(y >> 8); lcd_wr_data(y & 0xFF);
lcd_wr_data(end_y >> 8); lcd_wr_data(end_y & 0xFF);
lcd_wr_reg(0x2C);
for (row = 0; row < 16; row++) {
u8 byte1 = asc2_chi[index][row * 2];
u8 byte2 = asc2_chi[index][row * 2 + 1];
for (bit = 0; bit < 8; bit++) {
if ((byte2 >> bit) & 0x01) lcd_wr_data(color);
else lcd_wr_data(bgcolor);
}
for (bit = 0; bit < 8; bit++) {
if ((byte1 >> bit) & 0x01) lcd_wr_data(color);
else lcd_wr_data(bgcolor);}}}
void lcd_showString(u16 x, u16 y, const char *p, u16 color, u16 bgcolor) {
const char *end = p;
if (!p) return;
while (*end) {	
end++;}
end--;
while (end >= p) {
if (x > LCD_WIDTH - 8) {
x = 0;
y += 16;
}
lcd_showChar(x, y, *end, color, bgcolor);
x += 8;
end--;}}