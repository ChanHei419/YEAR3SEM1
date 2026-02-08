#include <stdlib.h>
#include <stdio.h>
#include "list.h"
struct listCDT{
listElementT h;
listADT t;};
listADT EmptyList(){
return NULL;}
listADT Cons(listElementT h1, listADT t1) {
listADT list = (listADT)malloc(sizeof(*list));
if(list == NULL){
fprintf(stderr, "Error(out of memory)\n");
exit(EXIT_FAILURE);}
list->h = h1;
list->t = t1;
return list;}
listElementT Head(listADT list) {
if (ListIsEmpty(list)) {
fprintf(stderr, "Error(head->empty))\n");
exit(EXIT_FAILURE);}
return list->h;}
listADT Tail(listADT list) {
if (ListIsEmpty(list)) {
fprintf(stderr, "Error(tail->empty))\n");
exit(EXIT_FAILURE);}
return list->t;}
int ListIsEmpty(listADT list){
return list == NULL;}