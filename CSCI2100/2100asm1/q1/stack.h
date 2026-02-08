/*
 * File: stack.h
 * Mary’s version
 */
#ifndef _STACK_H_
#define _STACK_H_
typedef struct cellT cellT;
typedef struct stackCDT *stackADT;
typedef char stackElementT;

stackADT EmptyStack(void);
void Push(stackADT stack, stackElementT element);
stackElementT Pop(stackADT stack);
stackElementT Peek(stackADT stack); /* This is new. */
int StackDepth(stackADT stack);
int StackIsEmpty(stackADT stack);

#endif