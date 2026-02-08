AREA RESET,DATA, READONLY  ; 定義一個名為 RESET 的資料區
    EXPORT __Vectors
    DCD 0x20002000         ; 設定初始堆疊指標 (Stack Pointer) 的位置
    DCD Reset_Handler      ; 設定程式的進入點 (Reset Handler)
__Vectors
AREA AFTER_RESET, CODE     ; 定義一個名為 AFTER_RESET 的程式碼區
    EXPORT Reset_Handler
Reset_Handler
    mov r8, #-5            ; 將 -5 這個數值存入暫存器 R8
loop
    ADDS r8, r8, #5        ; 將 R8 的值加上 5，並更新狀態旗標 (Flags)
    b loop                 ; 無條件跳轉到 loop 標籤，形成無限迴圈
    END                    ; 程式結束