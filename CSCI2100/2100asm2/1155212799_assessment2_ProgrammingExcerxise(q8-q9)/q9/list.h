#ifndef LIST_H
#define LIST_H
typedef struct listCDT *listADT;
typedef int listElementT;
listADT EmptyList(void);
listADT Cons(listElementT head, listADT tail);
listElementT Head(listADT list);
listADT Tail(listADT list);
int ListIsEmpty(listADT list);
#endif