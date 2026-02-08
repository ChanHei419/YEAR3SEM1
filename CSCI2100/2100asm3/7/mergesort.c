#include "mergesort.h"
#include <stdlib.h>
void TwoHalves(listADT L, listADT *pL1, listADT *pL2){
int len=ListLength(L);
if (len == 0) {
*pL1 = EmptyList();
*pL2 = EmptyList();
return;}
int mid=len/2;
listElementT*elements=malloc(len*sizeof(listElementT));
if(elements==NULL)exit(EXIT_FAILURE);
listADT current=L;
for(int i=0; i<len;i++){
elements[i]=Head(current);
current=Tail(current);
}
*pL1 = EmptyList();
for (int i = mid - 1; i >= 0; i--) {
*pL1 = Cons(elements[i], *pL1);}
*pL2 = EmptyList();
for (int i = len - 1; i >= mid; i--) {
*pL2 = Cons(elements[i], *pL2);}
free(elements);}
listADT merge(listADT L1, listADT L2) {
if (ListIsEmpty(L1)) return L2;
if (ListIsEmpty(L2)) return L1;
if (Head(L1) <= Head(L2)) {
return Cons(Head(L1), merge(Tail(L1), L2));
}else{
return Cons(Head(L2), merge(L1, Tail(L2)));}}
listADT mergesortList(listADT L){
if(ListLength(L)<=1){
return L;}
listADT L1, L2;
TwoHalves(L, &L1, &L2);
return merge(mergesortList(L1), mergesortList(L2));}