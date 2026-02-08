#include <stdio.h>
#include <stdlib.h>
#include "list.h"
void p(listADT l) {
printf("[");
listADT t = l;
while (!ListIsEmpty(t)) {
printf("%d", Head(t));
t = Tail(t);
if (!ListIsEmpty(t)) {
printf(", ");}}
printf("]\n");}
int main() {
printf("q8\n");
listADT l1 = EmptyList();
printf("ListIsEmpty: %d\n", ListIsEmpty(l1));
l1 = Cons(10, l1);
l1 = Cons(20, l1);
l1 = Cons(30, l1);
printf("l1: ");
p(l1);
printf("Head(l1): %d\n", Head(l1));
listADT l2 = Tail(l1);
printf("l2: ");
p(l2);
listADT l3 = Cons(99, l2);
printf("l3: ");
p(l3);
printf("l1 after: ");
p(l1);
listADT s = Cons(5, EmptyList());
printf("s: ");
p(s);
listADT t = Tail(s);
printf("Tail(s): ");
p(t);
printf("ListIsEmpty(t): %d\n", ListIsEmpty(t));
return 0;}