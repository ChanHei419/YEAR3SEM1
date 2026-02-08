#include <stdio.h>
#include "list.h"
#include "mergesort.h"
int main(){
listADT list1=Cons(9,Cons(3,Cons(8,Cons(7,EmptyList()))));
printf("Original list(list1):");
printList(list1);
listADT list2 = mergesortList(list1);
printf("Original list(list1)after sorting(unchanged):");
printList(list1);
printf("Sorted list(list2): ");
printList(list2);
return 0;
}