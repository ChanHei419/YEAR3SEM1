#ifndef __BOARD_H
#define __BOARD_H

#include "stm32f10x.h"

#define GRID_WIDTH      15
#define GRID_HEIGHT     15
#define BLOCK_SIZE      15
#define X_OFFSET        ((240 - (GRID_WIDTH * BLOCK_SIZE)) / 2)
#define Y_OFFSET        ((320 - (GRID_HEIGHT * BLOCK_SIZE)) / 2)
#define SNAKE_MAX_LENGTH 100
#define MAX_STONES       5
#define MAX_POISONS      2
#define NUM_LEVELS       6
#define BORDER_THICKNESS 2
#define NUM_STARS        60
#define STAR_SPEED       2

#define c_GRAY        0x8410
#define c_CYAN        0x07FF
#define c_PURPLE      0x801F
#define c_BROWN       0x8A22 
#define c_DARK_YELLOW 0xB580 
#define c_DARK_GREEN  0x0400 
#define c_DARKER_GREEN 0x0200 
#define c_ORANGE      0xFD20
#define c_RED         0xF800
#define c_GREEN       0x07E0
#define c_BLUE        0x001F
#define c_white       0xFFFF
#define c_black       0x0000
#define c_YELLOW      0xFFE0

typedef struct {
    int x;
    int y;
} Point;

typedef struct {
    Point body[SNAKE_MAX_LENGTH];
    int length;
    Point direction;
} Snake;

typedef struct {
    int initial_speed; 
    int walls_enabled;
    int num_stones;
    int moving_stones;
    int poison_enabled;
    int fruits_to_win;
} LevelConfig;

typedef struct {
    int x;
    int y;
    int speed;
    int visible; 
} Star;

typedef enum {
    START_SCREEN,
    INSTRUCTIONS,
    MENU,
    PLAYING,
    PAUSED, 
    GAME_OVER,
    GAME_WON
} GameState;

extern Snake snake;
extern Point fruit;
extern Point stones[MAX_STONES];
extern Point poisons[MAX_POISONS];
extern int num_active_poisons;
extern int score;
extern int high_score;
extern GameState current_state;
extern int current_level_index;
extern int selected_level_index;
extern int game_speed; 
extern int stone_move_counter;
extern int last_displayed_score;
extern int pause_selection;
extern int game_needs_full_redraw; 
extern int infinity_sub_level;
extern int last_speed_increase_score;
extern int last_displayed_infinity_level;
extern int direction_changed_this_tick;
extern Star stars[NUM_STARS];
extern LevelConfig levels[NUM_LEVELS];
extern volatile int led_flash_counter; 

void lcd_clear(u16 color);
void lcd_showString(u16 x, u16 y, u8 *p, u16 fColor, u16 bColor);
void lcd_fillRectangle(u16 color, u16 x, u16 w, u16 y, u16 h);
void TFTLCD_ShowString_Centered(u16 y, u8* str, u16 color, u16 bgcolor);

void lcd_drawRectangle(u16 color, u16 x, u16 width, u16 y, u16 height);

void Game_DrawStartScreen(void);
void Game_DrawInstructions(void); 
void Game_DrawMenu(void);
void Game_DrawMenuList(void);
void Game_DrawBoard(void); 
void Game_Draw(void);
void Game_DrawGameOverScreen(void);
void Game_DrawGameWonScreen(void);
void Game_DrawPauseMenu(void); 
void Game_RedrawScreen(void);

void Game_Init(void);
void Game_Update(void);
void Game_MoveStones(void);
void Infinity_Mode_Update(LevelConfig* dynamic_config);
void Infinity_Level_Up(int new_level);
void Generate_Safe_Point(Point* p);
int  Is_Occupied(Point p, int check_stones, int check_poisons);
void Update_HighScore(void);

void Starfield_Init(void);
void Starfield_Update(void);
void Starfield_Refresh_Visibility(void); // ??????

void delay_ms(u16 time);
void Play_Buzzer_Sound(u16 duration_ms);

#endif
