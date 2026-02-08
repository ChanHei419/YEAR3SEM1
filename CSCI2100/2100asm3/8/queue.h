#ifndef QUEUE_H
#define QUEUE_H
#include <stdlib.h>
typedef struct queueCDT *queueADT;
typedef int queueElementT;
queueADT EmptyQueue(void);
void Enqueue(queueADT Q, queueElementT element);
queueElementT Dequeue(queueADT Q);
int QueueIsEmpty(queueADT Q);
#endif