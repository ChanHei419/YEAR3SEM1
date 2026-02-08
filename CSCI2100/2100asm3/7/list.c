#include <stdio.h>
#include "list.h"
struct listCDT{
listElementT h;
listADT t;
};
listADT EmptyList(void){
return NULL;}
listADT Cons(listElementT h,listADT t){
listADT list = malloc(sizeof(*list));
if (list == NULL){exit(EXIT_FAILURE);}
list->h = h;
list->t = t;
return list;}
listElementT Head(listADT list){
if(ListIsEmpty(list)){exit(EXIT_FAILURE);}
return list->h;}
listADT Tail(listADT list){if(ListIsEmpty(list)){
exit(EXIT_FAILURE);}
return list->t;}
int ListIsEmpty(listADT list){
return list == NULL;}
int ListLength(listADT L){
if(ListIsEmpty(L)){
return 0;}else{
return (1 + ListLength(Tail(L)));}}
listADT Append(listADT L1,listElementT x){
if (ListIsEmpty(L1)) {
return Cons(x,EmptyList());
}else{return Cons(Head(L1), Append(Tail(L1), x));}}
void printList(listADT L){
printf("[");
listADT current=L;
while(!ListIsEmpty(current)){
printf("%d", Head(current));
current=Tail(current);
if(!ListIsEmpty(current)){
printf(", ");}}
printf("]\n");}