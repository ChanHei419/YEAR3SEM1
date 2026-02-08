#include "Board.h"
#include "IERG3810_TFTLCD.h"
#include "IERG3810_Buzzer.h"
#include "IERG3810_LED.h"
#include "JoyPad.h" 
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

Snake snake;
Point fruit;
Point stones[MAX_STONES];
Point poisons[MAX_POISONS];
int num_active_poisons = 0;

int score;
int high_score = 0;
GameState current_state;
int current_level_index;
int selected_level_index = 0;
int game_speed; 
int stone_move_counter = 0;

int last_displayed_score = -1;
int pause_selection = 0;
int game_needs_full_redraw = 0; 

int infinity_sub_level = 0;
int last_speed_increase_score = -1;
int last_displayed_infinity_level = -1;

int direction_changed_this_tick = 0;

Star stars[NUM_STARS];

LevelConfig levels[NUM_LEVELS] = {
    {200, 0, 0, 0, 0, 5},
    {180, 1, 0, 0, 0, 8},
    {160, 1, 3, 0, 0, 10},
    {140, 1, 4, 1, 0, 12},
    {120, 1, 5, 1, 1, 5},
    {150, 0, 0, 0, 0, -1}
};

const u8 apple_pixel_data[15][15] = {
    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    {0,0,0,0,0,1,1,1,1,1,0,0,0,0,0},
    {0,0,0,0,1,1,1,1,1,1,1,0,0,0,0},
    {0,0,0,1,1,1,1,1,1,1,1,1,0,0,0},
    {0,0,1,1,1,1,1,1,1,1,1,1,1,0,0},
    {0,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
    {0,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
    {0,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
    {0,1,1,4,1,1,1,1,1,1,1,1,1,1,0},
    {0,0,1,1,4,4,1,1,1,1,1,1,1,0,0},
    {0,0,0,1,1,1,1,1,1,1,1,1,0,0,0},
    {0,0,0,0,0,1,1,3,1,1,0,0,0,0,0},
    {0,0,0,0,0,0,2,2,3,0,0,0,0,0,0},
    {0,0,0,0,0,0,0,2,2,0,0,0,0,0,0},
    {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
};

const char* snake_pixel_art[40] = {
    "              BBBBBBB                   ",
    "             BBgGGGGgBBBB               ",
    "           BBgGGGGGGGGGGgBB             ",
    "          BdgBGGGGGGGGGGGGGB            ",
    "         BdgBGGGGGGgBGGGGGGgB           ",
    "         BdgBGGGGg4GGGGddGGgdB          ",
    "         BdgBGGGGGGdGdyYYdGGGB          ",
    "         BdgBgGgGGGdGdyYYYydGB          ",
    "         BdgBGGGGGBBGdyYYYYdGB          ",
    "         BdgYBG4BB4B GdyYYYYYdGB        ",
    "          BdgBBBRBB GdyYYYYdGGB         ",
    "          BdgYBRRGGGdyYYYYydGB          ",
    "           BdgRRBBBBdyyyyyydgdgB        ",
    "           BRRByyyyyYYYYydGGGB          ",
    "          BRRB BYYYYYYydGdgB            ",
    "         BRBRB BYYYYYYydGdgB            ",
    "         BBBRB ByyyyyydGdgB             ",
    "           BB   BYYYYdGdgB              ",
    "              BYYYYdGGB                 ",
    "             ByyyyyydGGB                ",
    "            ByyyyyydGGB                 ",
    "            ByyyyyydGGBBBBBBB           ",
    "          BByyyyyydGGGBggggggBB         ",
    "         BgBdyyyyydGGGgggggggB          ",
    "        BggByyyyydGGGGggggdgggdB        ",
    "       BggdgByyyyydGGGGgBBBGGGGggdB     ",
    "       BggdgByyyyydGGGGGGGGGGGGgGdgB    ",
    "      BggggBdyyyyddgGGGGGGGGGGGGdgB     ",
    "     BBggGGgdgdyyyydgGGGGGGGGGGgddB     ",
    "    BgBgGGGGdyyyydgGGGGGGGgggdggB       ",
    "   BggdBGGGGGGdBBBBddddddddddgggB       ",
    "  BGGGyBdyGGGGGGGGGGGGGGGGGGgggB        ",
    "  BGGYB BdyGGGGGGGGGGGGGGGGGGgdyB       ",
    "  BGB    BYYGGGGGGGGGGGGGGGGyyB         ",
    "   BGB     BByyyYyYyYyYyyyyBB           ",
    "    BB       BBBBBBBBBBBBBBBB           ",
    "                                        ",
    "                                        ",
    "                                        ",
    "                                        "
};

void lcd_clear(u16 color) { lcd_fillRectangle(color, 0, 240, 0, 320); }

void lcd_showString(u16 x, u16 y, u8 *p, u16 fColor, u16 bColor)
{
    while (*p != '\0') {
        if (x > 232) { x = 0; y += 16; }
        if (y > 304) { y = 0; x = 0; }
        lcd_showChar(x, y, *p, fColor, bColor);
        x += 8; p++;
    }
}

void TFTLCD_ShowString_Centered(u16 y, u8* str, u16 color, u16 bgcolor)
{
    int len = strlen((const char*)str);
    int x = (240 - (len * 8)) / 2;
    lcd_showString(x, y, str, color, bgcolor);
}

void Starfield_Init(void) {
    int i;
    for (i = 0; i < NUM_STARS; i++) {
        stars[i].x = rand() % 240;
        stars[i].y = rand() % 320;
        stars[i].speed = (rand() % 3) + 1;
        stars[i].visible = 0; 
    }
}

int Is_Pos_Safe_For_Star(int x, int y) {
    if (current_state == START_SCREEN) {
        if (y >= 25 && y <= 120) return 0; 
        if (y >= 125 && y <= 180) return 0;
        if (y >= 195 && y <= 240) return 0;
    }
    else if (current_state == INSTRUCTIONS) {
        if (x >= 10 && x <= 230 && y >= 20 && y <= 280) return 0;
    }
    else if (current_state == MENU) {
        if (y >= 70 && y <= 270) {
            if (x >= 10 && x <= 230) return 0; 
        }
    }
    return 1; 
}

void Starfield_Refresh_Visibility(void) {
    int i;
    for (i = 0; i < NUM_STARS; i++) {
        if (!Is_Pos_Safe_For_Star(stars[i].x, stars[i].y)) {
            stars[i].visible = 0;
        }
    }
}

void Starfield_Update(void) {
    int i;
    int is_safe_old, is_safe_new;
    
    for (i = 0; i < NUM_STARS; i++) {
        if (stars[i].visible) {
            is_safe_old = Is_Pos_Safe_For_Star(stars[i].x, stars[i].y);
            
            if (is_safe_old) {
                lcd_fillRectangle(c_black, stars[i].x, 1, stars[i].y, 1);
            }
        }

        stars[i].y += stars[i].speed;
        if (stars[i].y >= 320) {
            stars[i].y = 0;
            stars[i].x = rand() % 240;
            stars[i].speed = (rand() % 3) + 1;
        }

        is_safe_new = Is_Pos_Safe_For_Star(stars[i].x, stars[i].y);

        if (is_safe_new) {
            lcd_fillRectangle(c_white, stars[i].x, 1, stars[i].y, 1);
            stars[i].visible = 1;
        } else {
            stars[i].visible = 0;
        }
    }
}

void Game_DrawApple(u16 x, u16 y, u16 size, u16 bgcolor) {
    int i, j;
    u16 color;
    for (i = 0; i < 15; i++) { 
        for (j = 0; j < 15; j++) { 
            u8 pixel_code = apple_pixel_data[i][j];
            if (pixel_code == 0) color = bgcolor;
            else {
                switch(pixel_code) {
                    case 1: color = c_RED; break;
                    case 2: color = c_GREEN; break;
                    case 3: color = c_BROWN; break;
                    case 4: color = c_white; break; 
                    default: color = bgcolor; break;
                }
            }
            if (color == bgcolor && size > 1) continue;
            lcd_fillRectangle(color, x + j * size, size, y + i * size, size);
        }
    }
}

void Game_DrawSnakeArt(u16 x, u16 y, u16 size, u16 bgcolor) {
    int i, j;
    for (i = 0; i < 40; i++) { 
        for (j = 0; j < 40; j++) { 
            char pixel_code = snake_pixel_art[39 - i][j];
            u16 color = bgcolor;
            switch(pixel_code) {
                case 'B': color = c_black; break;
                case 'R': color = c_RED; break;
                case 'Y': color = c_YELLOW; break;
                case 'G': color = c_GREEN; break;
                case '4': color = c_white; break;
                case 'y': color = c_DARK_YELLOW; break;
                case 'g': color = c_DARK_GREEN; break;
                case 'd': color = c_DARKER_GREEN; break;
                default:  break;
            }
            if (color != bgcolor) {
                lcd_fillRectangle(color, x + j * size, size, y + i * size, size);
            }
        }
    }
}

void Game_DrawStartScreen(void)
{
    char cuid1[] = "1155213082", cuid2[] = "1155212799";
    u8 name1_indices[] = {0, 1, 2}, name2_indices[] = {3, 4, 5};
    u16 i, start_x, name_start_x, total_width;

    lcd_clear(c_black);
    Game_DrawApple(42, 40, 3, c_black); 
    Game_DrawSnakeArt(117, 30, 2, c_black);

    TFTLCD_ShowString_Centered(130, (u8*)"Press START to begin", c_YELLOW, c_black);
    TFTLCD_ShowString_Centered(160, (u8*)"Snake Game", c_GREEN, c_black);
    
    total_width = 10 * 8 + 10 + 3 * 16;
    start_x = (240 - total_width) / 2;
    name_start_x = start_x + 10 * 8 + 10;
    
    for (i = 0; i < 10; i++) lcd_showChar(start_x + i * 8, 200, cuid1[i], c_white, c_black);
    for (i = 0; i < 3; i++) lcd_showChinChar(name_start_x + i * 16, 200, name1_indices[i], c_white, c_black);
    
    for (i = 0; i < 10; i++) lcd_showChar(start_x + i * 8, 220, cuid2[i], c_white, c_black);
    for (i = 0; i < 3; i++) lcd_showChinChar(name_start_x + i * 16, 220, name2_indices[i], c_white, c_black);
    
    Starfield_Refresh_Visibility(); 
}

void Game_DrawInstructions(void)
{
    lcd_clear(c_black);
    TFTLCD_ShowString_Centered(30, (u8*)"Press START to Continue", c_GREEN, c_black);
    lcd_showString(30, 70, (u8*)"(Except Level 1)", c_YELLOW, c_black);
    lcd_showString(10, 86, (u8*)"5. Don't hit Walls", c_YELLOW, c_black);
    lcd_showString(10, 110, (u8*)"4. Avoid PURPLE Poison", c_PURPLE, c_black);
    lcd_showString(10, 140, (u8*)"3. Avoid GRAY Stones", c_GRAY, c_black);
    lcd_showString(10, 170, (u8*)"2. Eat RED Apples", c_RED, c_black);
    lcd_showString(10, 200, (u8*)"1. Use D-Pad to Move", c_white, c_black);
    TFTLCD_ShowString_Centered(260, (u8*)"INSTRUCTIONS", c_CYAN, c_black);
    
    Starfield_Refresh_Visibility();
}

void Game_DrawMenuList(void)
{
    int i; char level_name[20]; u16 color, bgcolor;
    const char* level_names[] = {"Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Infinity Mode"};
    for (i = 0; i < NUM_LEVELS; i++)
    {
        color = (i == selected_level_index) ? c_black : c_white;
        bgcolor = (i == selected_level_index) ? c_YELLOW : c_black;
        sprintf(level_name, "%-13s", level_names[i]);
        TFTLCD_ShowString_Centered(140 + (NUM_LEVELS - 1 - i) * 20, (u8*)level_name, color, bgcolor);
    }
}

void Game_DrawMenu(void)
{
    lcd_clear(c_black);
    TFTLCD_ShowString_Centered(80, (u8*)"Select a Level", c_GREEN, c_black);
    TFTLCD_ShowString_Centered(110, (u8*)"Use UP/DOWN to Select", c_YELLOW, c_black);
    Game_DrawMenuList();
    
    Starfield_Refresh_Visibility(); 
}

void Game_DrawBoard(void)
{
    u16 corner_size = 6;
    u16 corner_color = c_ORANGE;

    lcd_fillRectangle(c_RED, X_OFFSET - BORDER_THICKNESS, BORDER_THICKNESS, Y_OFFSET, GRID_HEIGHT * BLOCK_SIZE);
    lcd_fillRectangle(c_RED, X_OFFSET + GRID_WIDTH * BLOCK_SIZE, BORDER_THICKNESS, Y_OFFSET, GRID_HEIGHT * BLOCK_SIZE);
    lcd_fillRectangle(c_RED, X_OFFSET - BORDER_THICKNESS, GRID_WIDTH * BLOCK_SIZE + 2 * BORDER_THICKNESS, Y_OFFSET - BORDER_THICKNESS, BORDER_THICKNESS);
    lcd_fillRectangle(c_RED, X_OFFSET - BORDER_THICKNESS, GRID_WIDTH * BLOCK_SIZE + 2 * BORDER_THICKNESS, Y_OFFSET + GRID_HEIGHT * BLOCK_SIZE, BORDER_THICKNESS);
    
    lcd_fillRectangle(corner_color, X_OFFSET - BORDER_THICKNESS - 2, corner_size, Y_OFFSET - BORDER_THICKNESS - 2, corner_size);
    lcd_fillRectangle(corner_color, X_OFFSET + GRID_WIDTH * BLOCK_SIZE + BORDER_THICKNESS + 2 - corner_size, corner_size, Y_OFFSET - BORDER_THICKNESS - 2, corner_size);
    lcd_fillRectangle(corner_color, X_OFFSET - BORDER_THICKNESS - 2, corner_size, Y_OFFSET + GRID_HEIGHT * BLOCK_SIZE + BORDER_THICKNESS + 2 - corner_size, corner_size);
    lcd_fillRectangle(corner_color, X_OFFSET + GRID_WIDTH * BLOCK_SIZE + BORDER_THICKNESS + 2 - corner_size, corner_size, Y_OFFSET + GRID_HEIGHT * BLOCK_SIZE + BORDER_THICKNESS + 2 - corner_size, corner_size);
}

void Game_Draw(void)
{
    int i;
    char text_buffer[30];
    int num_stones_to_draw;
    int poison_enabled;

    num_stones_to_draw = (current_level_index == NUM_LEVELS - 1) ? (infinity_sub_level >= 2 ? (infinity_sub_level == 2 ? 3 : (infinity_sub_level == 3 ? 4 : 5)) : 0) : levels[current_level_index].num_stones;
    poison_enabled = (current_level_index == NUM_LEVELS - 1) ? (infinity_sub_level >= 4) : levels[current_level_index].poison_enabled;

    if (score != last_displayed_score) {
        sprintf(text_buffer, "Score: %d  ", score);
        lcd_showString(5, 5, (u8*)text_buffer, c_white, c_black);
        last_displayed_score = score;
    }
    
    if (current_level_index == NUM_LEVELS - 1) {
        if (infinity_sub_level != last_displayed_infinity_level) {
            if (infinity_sub_level < 5) {
                sprintf(text_buffer, "Level %d", infinity_sub_level + 1);
            } else {
                sprintf(text_buffer, "Speed Up Mode");
            }
            lcd_fillRectangle(c_black, 0, 240, 300, 16);
            TFTLCD_ShowString_Centered(300, (u8*)text_buffer, c_CYAN, c_black);
            last_displayed_infinity_level = infinity_sub_level;
        }
    }
    
    for (i = 0; i < num_stones_to_draw; i++) lcd_fillRectangle(c_GRAY, stones[i].x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE - 1, stones[i].y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE - 1);
    
    if (poison_enabled) {
        for (i = 0; i < num_active_poisons; i++) {
            lcd_fillRectangle(c_PURPLE, poisons[i].x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE - 1, poisons[i].y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE - 1);
        }
    }

    Game_DrawApple(fruit.x * BLOCK_SIZE + X_OFFSET, fruit.y * BLOCK_SIZE + Y_OFFSET, 1, c_black);
    
    for (i = 0; i < snake.length; i++) lcd_fillRectangle((i == 0) ? c_CYAN : c_GREEN, snake.body[i].x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE - 1, snake.body[i].y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE - 1);
}

void Game_DrawGameOverScreen(void)
{
    char final_score_text[20], high_score_text[20];
    sprintf(final_score_text, "Final Score: %d", score);
    lcd_clear(c_RED);
    TFTLCD_ShowString_Centered(110, (u8*)"GAME OVER", c_white, c_RED);
    TFTLCD_ShowString_Centered(140, (u8*)final_score_text, c_white, c_RED);
    if (current_level_index == NUM_LEVELS - 1) {
        sprintf(high_score_text, "High Score: %d", high_score);
        TFTLCD_ShowString_Centered(160, (u8*)high_score_text, c_white, c_RED);
    }
    TFTLCD_ShowString_Centered(190, (u8*)"Press START to Continue", c_YELLOW, c_RED);
}

void Game_DrawGameWonScreen(void)
{
    char final_score_text[20];
    sprintf(final_score_text, "Final Score: %d", score);
    lcd_clear(c_BLUE);
    TFTLCD_ShowString_Centered(110, (u8*)"YOU WIN!", c_YELLOW, c_BLUE);
    TFTLCD_ShowString_Centered(140, (u8*)final_score_text, c_white, c_BLUE);
    TFTLCD_ShowString_Centered(190, (u8*)"Press START to Continue", c_white, c_BLUE);
}

void Game_DrawPauseMenu(void)
{
    u16 res_f_color, res_b_color, quit_f_color, quit_b_color;
    lcd_fillRectangle(c_GRAY, 50, 140, 120, 80);
    lcd_drawRectangle(c_white, 50, 140, 120, 80);
    TFTLCD_ShowString_Centered(130, (u8*)"Game Paused", c_white, c_GRAY);
    res_f_color = (pause_selection == 0) ? c_black : c_white; res_b_color = (pause_selection == 0) ? c_YELLOW : c_GRAY;
    quit_f_color = (pause_selection == 1) ? c_black : c_white; quit_b_color = (pause_selection == 1) ? c_YELLOW : c_GRAY;
    lcd_showString(70, 165, (u8*)"Resume", res_f_color, res_b_color);
    lcd_showString(130, 165, (u8*)"Quit", quit_f_color, quit_b_color);
}

void Game_RedrawScreen(void)
{
    char text_buffer[30];
    lcd_clear(c_black);
    Game_DrawBoard();
    if (current_level_index == NUM_LEVELS - 1) {
        sprintf(text_buffer, "High Score: %d", high_score);
        lcd_showString(5, 25, (u8*)text_buffer, c_white, c_black);
        last_displayed_infinity_level = -1;
    }
    lcd_showString(240 - 15 * 8, 5, (u8*)"Press SELECT Pause", c_YELLOW, c_black);
    last_displayed_score = -1;
    Game_Draw();
}

void Generate_Safe_Point(Point* p)
{
    Point temp_p;
    int num_stones;
    int poison_enabled;
    
    num_stones = (current_level_index == NUM_LEVELS - 1) ? (infinity_sub_level >= 2 ? (infinity_sub_level == 2 ? 3 : (infinity_sub_level == 3 ? 4 : 5)) : 0) : levels[current_level_index].num_stones;
    poison_enabled = (current_level_index == NUM_LEVELS - 1) ? (infinity_sub_level >= 4) : levels[current_level_index].poison_enabled;
    
    do {
        temp_p.x = rand() % GRID_WIDTH;
        temp_p.y = rand() % GRID_HEIGHT;
    } while (Is_Occupied(temp_p, num_stones > 0, poison_enabled));
    
    *p = temp_p;
}

int Is_Occupied(Point p, int check_stones, int check_poisons)
{
    int i;
    int num_stones_to_check;

    num_stones_to_check = (current_level_index == NUM_LEVELS - 1) ? (infinity_sub_level >= 2 ? (infinity_sub_level == 2 ? 3 : (infinity_sub_level == 3 ? 4 : 5)) : 0) : levels[current_level_index].num_stones;

    for (i = 0; i < snake.length; i++) if (p.x == snake.body[i].x && p.y == snake.body[i].y) return 1;
    if (check_stones) for (i = 0; i < num_stones_to_check; i++) if (p.x == stones[i].x && p.y == stones[i].y) return 1;
    
    if (check_poisons) {
        for (i = 0; i < num_active_poisons; i++) {
            if (p.x == poisons[i].x && p.y == poisons[i].y) return 1;
        }
    }
    
    if (p.x == fruit.x && p.y == fruit.y) return 1;
    
    return 0;
}

void Game_Init(void)
{
    int i;
    char text_buffer[20];
    LevelConfig* level = &levels[current_level_index];

    score = 0;
    last_displayed_score = -1;
    game_speed = level->initial_speed;
    stone_move_counter = 0;
    
    num_active_poisons = 0;
    
    if (current_level_index == NUM_LEVELS - 1) {
        infinity_sub_level = 0;
        last_speed_increase_score = -1;
        last_displayed_infinity_level = -1;
    }
    
    snake.length = 3;
    snake.body[0].x = GRID_WIDTH / 2; snake.body[0].y = GRID_HEIGHT / 2;
    snake.body[1].x = GRID_WIDTH / 2 - 1; snake.body[1].y = GRID_HEIGHT / 2;
    snake.body[2].x = GRID_WIDTH / 2 - 2; snake.body[2].y = GRID_HEIGHT / 2;
    snake.direction.x = 1; snake.direction.y = 0;

    for (i = 0; i < level->num_stones; i++) Generate_Safe_Point(&stones[i]);
    
    if (level->poison_enabled) {
        for (i = 0; i < MAX_POISONS; i++) {
            Generate_Safe_Point(&poisons[i]);
        }
        num_active_poisons = MAX_POISONS;
    }
    
    Generate_Safe_Point(&fruit);
    
    lcd_clear(c_black);
    Game_DrawBoard(); 
    
    if (current_level_index == NUM_LEVELS - 1) {
        sprintf(text_buffer, "High Score: %d", high_score);
        lcd_showString(5, 25, (u8*)text_buffer, c_white, c_black);
    }
    
    lcd_showString(240 - 15 * 8, 5, (u8*)"Press SELECT Pause", c_YELLOW, c_black);
}

void Infinity_Level_Up(int new_level) {
    int i; 
    switch (new_level) {
        case 0: case 1: break;
        case 2:
            for (i = 0; i < 3; i++) Generate_Safe_Point(&stones[i]);
            break;
        case 3:
            Generate_Safe_Point(&stones[3]);
            break;
        case 4: 
            Generate_Safe_Point(&stones[4]);
            for (i = 0; i < MAX_POISONS; i++) {
                Generate_Safe_Point(&poisons[i]);
            }
            num_active_poisons = MAX_POISONS;
            break;
        default: break;
    }
    game_needs_full_redraw = 1;
}

void Infinity_Mode_Update(LevelConfig* dynamic_config) {
    int new_sub_level = score / 5;
    
    if (new_sub_level > infinity_sub_level) {
        Infinity_Level_Up(new_sub_level);
        infinity_sub_level = new_sub_level;
    }
    
    *dynamic_config = levels[NUM_LEVELS - 1]; 
    
    if (infinity_sub_level >= 1) { dynamic_config->walls_enabled = 1; }
    if (infinity_sub_level >= 2) { dynamic_config->num_stones = 3; }
    if (infinity_sub_level >= 3) { dynamic_config->num_stones = 4; dynamic_config->moving_stones = 1; }
    if (infinity_sub_level >= 4) { dynamic_config->num_stones = 5; dynamic_config->poison_enabled = 1; }
    
    if (score >= 25 && score % 5 == 0 && score != last_speed_increase_score) {
        if (game_speed > 60) game_speed -= 20; 
        last_speed_increase_score = score;
    }
}

void Game_MoveStones(void)
{
    int i, dir_idx;
    Point new_pos;
    Point directions[] = {{0, 1}, {0, -1}, {1, 0}, {-1, 0}};
    int num_stones_to_move; 

    num_stones_to_move = (current_level_index == NUM_LEVELS - 1) ? (infinity_sub_level >= 3 ? (infinity_sub_level == 3 ? 4 : 5) : 0) : levels[current_level_index].num_stones;

    for (i = 0; i < num_stones_to_move; i++)
    {
        dir_idx = rand() % 4;
        new_pos.x = stones[i].x + directions[dir_idx].x;
        new_pos.y = stones[i].y + directions[dir_idx].y;
        if (new_pos.x >= 0 && new_pos.x < GRID_WIDTH && new_pos.y >= 0 && new_pos.y < GRID_HEIGHT && !Is_Occupied(new_pos, 1, 1))
        {
            lcd_fillRectangle(c_black, stones[i].x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE, stones[i].y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE);
            stones[i] = new_pos;
        }
    }
}

void Game_Update(void)
{
    Point next_head;
    int i;
    LevelConfig static_level_config;
    LevelConfig* level;
    int eaten_poison_index = -1; 
    Point tail_point;

    direction_changed_this_tick = 0;

    if (current_level_index == NUM_LEVELS - 1) {
        level = &static_level_config;
        Infinity_Mode_Update(level);
    } else {
        level = &levels[current_level_index];
    }

    next_head.x = snake.body[0].x + snake.direction.x;
    next_head.y = snake.body[0].y + snake.direction.y;

    if (level->walls_enabled) {
        if (next_head.x < 0 || next_head.x >= GRID_WIDTH || next_head.y < 0 || next_head.y >= GRID_HEIGHT) {
            current_state = GAME_OVER; return;
        }
    } else {
        if (next_head.x < 0) next_head.x = GRID_WIDTH - 1;
        if (next_head.x >= GRID_WIDTH) next_head.x = 0;
        if (next_head.y < 0) next_head.y = GRID_HEIGHT - 1;
        if (next_head.y >= GRID_HEIGHT) next_head.y = 0;
    }

    for (i = 0; i < level->num_stones; i++) if (next_head.x == stones[i].x && next_head.y == stones[i].y) { current_state = GAME_OVER; return; }
    
    if (snake.length >= 4) {
        for (i = 1; i < snake.length; i++) {
            if (next_head.x == snake.body[i].x && next_head.y == snake.body[i].y) {
                current_state = GAME_OVER; return;
            }
        }
    }

    if (level->poison_enabled) {
        for (i = 0; i < num_active_poisons; i++) {
            if (next_head.x == poisons[i].x && next_head.y == poisons[i].y) {
                eaten_poison_index = i;
                break;
            }
        }
    }

    if (eaten_poison_index != -1) { 
        Play_Buzzer_Sound(150);

        lcd_fillRectangle(c_black, poisons[eaten_poison_index].x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE, poisons[eaten_poison_index].y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE);
        
        tail_point = snake.body[snake.length - 1];
        lcd_fillRectangle(c_black, tail_point.x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE, tail_point.y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE);

        if (snake.length > 1) {
            tail_point = snake.body[snake.length - 2];
            lcd_fillRectangle(c_black, tail_point.x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE, tail_point.y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE);
        }
        
        snake.length--;

        if (snake.length < 2) {
            current_state = GAME_OVER;
            return;
        }

        if (current_level_index == NUM_LEVELS - 1 && score >= 25) {
            Generate_Safe_Point(&poisons[eaten_poison_index]);
        } else { 
            poisons[eaten_poison_index] = poisons[num_active_poisons - 1];
            num_active_poisons--;
        }

    } else if (next_head.x == fruit.x && next_head.y == fruit.y) {
        lcd_fillRectangle(c_black, fruit.x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE, fruit.y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE);
        
        score++;
        if (snake.length < SNAKE_MAX_LENGTH) snake.length++;
        Play_Buzzer_Sound(20);
        
        led_flash_counter = 200; 
        
        Generate_Safe_Point(&fruit);
        
    } else {
        tail_point = snake.body[snake.length - 1];
        lcd_fillRectangle(c_black, tail_point.x * BLOCK_SIZE + X_OFFSET, BLOCK_SIZE, tail_point.y * BLOCK_SIZE + Y_OFFSET, BLOCK_SIZE);
    }

    for (i = snake.length - 1; i > 0; i--) snake.body[i] = snake.body[i - 1];
    snake.body[0] = next_head;
    
    if (level->moving_stones) {
        stone_move_counter++;
        if (stone_move_counter > 30) { Game_MoveStones(); stone_move_counter = 0; }
    }

    if (level->fruits_to_win != -1 && score >= level->fruits_to_win) {
        current_state = GAME_WON; return;
    }
}

void Update_HighScore(void) { if (current_level_index == NUM_LEVELS - 1) { if (score > high_score) { high_score = score; } } }

void Play_Buzzer_Sound(u16 duration_ms) { Buzzer_On(); delay_ms(duration_ms); Buzzer_Off(); }

void delay_ms(u16 time) { u32 i; for(i = 0; i < time * 10000; i++); }
