#ifndef __IERG3810_TFTLCD_H
#define __IERG3810_TFTLCD_H

#include "stm32f10x.h"

typedef unsigned char u8;
typedef unsigned short u16;
typedef unsigned int u32;
typedef signed char s8;
typedef volatile unsigned short vu16;

#define LCD_WIDTH 240
#define LCD_HEIGHT 320

typedef struct {
vu16 LCD_REG;
vu16 LCD_RAM;
} LCD_TypeDef;

#define LCD_BASE ((u32)(0x6C000000 | 0x000007FE))
#define LCD      ((LCD_TypeDef *)LCD_BASE)

#define WHITE     0xFFFF
#define BLACK     0x0000
#define BLUE      0x001F
#define RED       0xF800
#define MAGENTA   0xF81F
#define GREEN     0x07E0
#define CYAN      0x7FFF
#define YELLOW    0xFFE0
#define LIGHTBLUE 0x7D7C

void lcd_init(void);
void lcd_drawDot(u16 x, u16 y, u16 color);
void lcd_fillRectangle(u16 color, u16 start_x, u16 length_x, u16 start_y, u16 length_y);
void lcd_sevenSegment(u16 color, u16 start_x, u16 start_y, u8 digit);
void lcd_showChar(u16 x, u16 y, u8 ascii, u16 color, u16 bgcolor);
void lcd_showString(u16 x, u16 y, const char *p, u16 color, u16 bgcolor);
void lcd_showChinChar(u16 x, u16 y, u8 index, u16 color, u16 bgcolor);
void delay_ms(u32 count);

#endif