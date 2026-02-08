#include "stm32f10x.h"
#include "IERG3810_Clock.h"
#include "IERG3810_USART.h"
#include "IERG3810_LED.h"
#include "IERG3810_Buzzer.h"
#include "drawing.h"
#include "game.h"
#include "IERG3810_KEY_PS2.h"
#include "IERG3810_TIMER.h"

// ??????
extern volatile u8 game_update_flag;
extern volatile u8 slow_event_flag;

// ?? Lab 2 ???,????
void Usart_Debug(char *s) {
    usart_print_txe(1, s);
}

int main(void) {
    u8 ps2_scancode;
    
    // ???????
    // ???? Lab ?????????
    clocktree_init();      // Exp2.1
    usart1_init(72, 9600); // Exp2.2 for debug
    IERG3810_LED_Init();     // Exp1.5
    IERG3810_Buzzer_Init();  // Exp1.5
    Drawing_Init();        // Exp3.x
    PS2_Init();            // Exp4.5
    
    // ??????
    // TIM3 ??????,?????? Level 1
    // 72MHz / (7199+1) / (8999+1) = 1Hz
    TIM3_Init(8999, 7199); 
    // SysTick ?????? (10Hz)
    SysTick_Init();
    
    // ????????,?? Exp4.x
    // nvic_setPriorityGroup(5); // ????,??????
    
    Usart_Debug("System Initialized. Welcome to Greedy Snake!\r\n");

    while (1) {
        // ??PS/2????
        ps2_scancode = PS2_Get_ScanCode();
        if (ps2_scancode != 0) {
            Game_HandlePS2Input(ps2_scancode);
        }

        // ??????
        switch (gameState) {
            case START_SCREEN:
                Draw_StartScreen();
                // ????,? Game_HandlePS2Input ??
                break;
                
            case GAME_PLAY:
                // ? TIM3 ????????
                if (game_update_flag) {
                    game_update_flag = 0;
                    
                    // ? SysTick ????????
                    if (slow_event_flag) {
                        slow_event_flag = 0;
                        // ???????????????,
                        // ????,?????? Game_Update() ?,
                        // ??????????
                    }
                    
                    Game_Update();
                    Draw_GameScreen();
                }
                break;
                
            case GAME_OVER:
                Draw_GameOverScreen();
                // ????,? Game_HandlePS2Input ??
                break;

            case GAME_WIN:
                Draw_GameWinScreen();
                // ????,? Game_HandlePS2Input ??
                break;
        }
    }
}