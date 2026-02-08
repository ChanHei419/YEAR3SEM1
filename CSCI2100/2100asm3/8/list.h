#ifndef LIST_H
#define LIST_H
#include <stdlib.h>
typedef struct listCDT*listADT;
typedef int listElementT;
listADT EmptyList(void);
listADT Cons(listElementT h, listADT t);
listElementT Head(listADT list);
listADT Tail(listADT list);
int ListIsEmpty(listADT list);
int ListLength(listADT L);
listADT Append(listADT L1, listElementT x);
void printList(listADT L);
#endif