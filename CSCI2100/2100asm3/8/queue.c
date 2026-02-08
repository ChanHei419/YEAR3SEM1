#include <stdio.h>
#include "queue.h"
typedef struct queueNode {
queueElementT data;
struct queueNode *next;}
queueNode;
struct queueCDT {
queueNode *front;
queueNode *rear;};
queueADT EmptyQueue(void) {
queueADT Q = malloc(sizeof(*Q));
if (Q == NULL) exit(EXIT_FAILURE);
Q->front = NULL;
Q->rear = NULL;
return Q;}
void Enqueue(queueADT Q, queueElementT element){
queueNode *newNode = malloc(sizeof(*newNode));
if (newNode == NULL) exit(EXIT_FAILURE);
newNode->data = element;
newNode->next = NULL;
if (Q->rear == NULL){ 
Q->front = newNode;
Q->rear = newNode;
} else {
Q->rear->next = newNode;
Q->rear = newNode;}}
queueElementT Dequeue(queueADT Q) {
if (QueueIsEmpty(Q)) {
exit(EXIT_FAILURE); }
queueNode *temp = Q->front;
queueElementT data = temp->data;
Q->front = Q->front->next;
if (Q->front == NULL) {
Q->rear = NULL;}
free(temp);
return data;}
int QueueIsEmpty(queueADT Q) {
return Q->front == NULL;}