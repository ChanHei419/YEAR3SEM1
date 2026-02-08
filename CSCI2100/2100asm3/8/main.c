#include <stdio.h>
#include <ctype.h>
#include "queue.h"
#include "list.h"
listADT queueToList(queueADT Q){
if (QueueIsEmpty(Q)) {
return EmptyList();
}else{
queueElementT elem = Dequeue(Q);
listADT tailList = queueToList(Q);
return Cons(elem, tailList);}}
int main(){
queueADT Q=EmptyQueue();
printf("Enter integers in a line(like 1 4 2 3 5) and then press enter: ");
int num;
char ch;
while (scanf("%d%c", &num, &ch) == 2) {
Enqueue(Q,num);
if (ch=='\n'){
break;}}
listADT resultList = queueToList(Q);
printf("return list: ");
printList(resultList);
return 0;
}