#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include "symtab.h"
#define nrBuckets 200
static int deleted_marker_variable; 
#define DELETED_MARKER ((void *) &deleted_marker_variable)
typedef struct cellT {
char *key; 
void *value;
}cellT;
struct symtabCDT{
cellT *bucket[nrBuckets];
};
int Hash(char *s){
unsigned long hashcode = 0UL;
for (int i = 0; s[i] != '\0'; i++) {
hashcode = (hashcode << 7) + (unsigned long)s[i];
}
return (int)(hashcode % nrBuckets);}
int Hash2(char *s) {
unsigned long hashcode = 0UL;
for (int i = 0; s[i] != '\0'; i++) {
hashcode = (hashcode << 5) + (unsigned long)s[i];
}
int step = (int)(101 - (hashcode % 101));
return step > 0 ? step : 1;}
char *strdup(const char *s) {
char *d = malloc(strlen(s) + 1);
if (d == NULL) return NULL;
strcpy(d, s);
return d;}
symtabADT EmptySymbolTable() {
symtabADT table = (symtabADT)malloc(sizeof(*table));
if (table == NULL) {
fprintf(stderr, "Error: Out of memory\n");
exit(EXIT_FAILURE);}
for (int i = 0; i < nrBuckets; i++) {
table->bucket[i] = NULL;}
return table;}
void Enter(symtabADT table, char *key, void *value){
int index = Hash(key);
int step = Hash2(key);
int first_deleted = -1;
for (int i = 0; i < nrBuckets; i++) {
cellT *current_cell = table->bucket[index];
if (current_cell == NULL) {
int insert_pos = (first_deleted != -1) ? first_deleted : index;
cellT *new_cell = (cellT *)malloc(sizeof(cellT));
if (new_cell == NULL) {
fprintf(stderr, "Error: Out of memory for new cell\n");
exit(EXIT_FAILURE); }
new_cell->key = strdup(key);
if (new_cell->key == NULL) {
fprintf(stderr, "Error: Out of memory for key\n");
free(new_cell);
exit(EXIT_FAILURE);}
new_cell->value = value;
table->bucket[insert_pos] = new_cell;
return;}
if (current_cell == DELETED_MARKER){if (first_deleted == -1) {
first_deleted = index;}
} else if (strcmp(current_cell->key, key) == 0) {
current_cell->value = value;
return;}
index = (index + step) % nrBuckets;}
fprintf(stderr, "Error: Symbol table is full.\n");
exit(EXIT_FAILURE);
}
void *Lookup(symtabADT table, char *key) {
int index = Hash(key);
int step = Hash2(key);
for (int i = 0; i < nrBuckets; i++) {
cellT *current_cell = table->bucket[index];
if (current_cell == NULL) {
return NULL;
}if (current_cell != DELETED_MARKER && strcmp(current_cell->key, key) == 0) {
return current_cell->value;
}index = (index + step) % nrBuckets;}return NULL;}
void Delete(symtabADT table, char *key) {
int index = Hash(key);
int step = Hash2(key);
for (int i = 0; i < nrBuckets; i++) {
cellT *current_cell = table->bucket[index];
if (current_cell == NULL){
return;}
if (current_cell != DELETED_MARKER && strcmp(current_cell->key, key) == 0){
free(current_cell->key);
free(current_cell);
table->bucket[index] = DELETED_MARKER;
return;}
index = (index + step) % nrBuckets;}}
int SymTabIsEmpty(symtabADT table){
for (int i = 0; i < nrBuckets; i++) {
if (table->bucket[i] != NULL && table->bucket[i] != DELETED_MARKER) {
return 0;}}
return 1;}
void forEachEntryDo(symtabFnT fn, symtabADT table) {
for (int i = 0; i < nrBuckets; i++) {
cellT *current_cell = table->bucket[i];
if (current_cell != NULL && current_cell != DELETED_MARKER) {
fn(current_cell->key, current_cell->value);}}}