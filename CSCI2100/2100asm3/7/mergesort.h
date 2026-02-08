#ifndef MERGESORT_H
#define MERGESORT_H
#include "list.h"
listADT mergesortList(listADT L);
void TwoHalves(listADT L, listADT *pL1, listADT *pL2);
listADT merge(listADT L1, listADT L2);
#endif