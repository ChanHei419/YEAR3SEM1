#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "symtab.h"
#include "list.h"
#include "prizeFnT.h"
typedef struct {
int form;
listADT gamesPlayed;
prizeFnT p;
} *playerDataT;
void printPlayerPrize(char *name, void *value) {
playerDataT player = (playerDataT)value;
prizeT prize = player->p(player->form, player->gamesPlayed);
char *prizeString;
switch (prize) {
case grandPrize: prizeString = "Grand Prize"; break;
case largePrize: prizeString = "Large Prize"; break;
case mediumPrize: prizeString = "Medium Prize"; break;
case smallPrize: prizeString = "Small Prize"; break;
default: prizeString = "no prize"; break;}
printf("%s receives a %s.\n", name, prizeString);}
void freePlayerData(char *name, void *value) {
playerDataT player = (playerDataT)value;
listADT current = player->gamesPlayed;
while (current != NULL) {
listADT temp = Tail(current);
free(current);
current = temp;}
free(player);}
int main() {
symtabADT playerTable = EmptySymbolTable();
FILE *infile;
char nameBuffer[100];
char lineBuffer[200];
infile = fopen("carnivalinput.txt", "r");
if (infile == NULL) {
fprintf(stderr, "Error: Cannot open carnivalinput.txt\n");
return 1;
}while (fgets(nameBuffer, sizeof(nameBuffer), infile) != NULL) {
nameBuffer[strcspn(nameBuffer, "\n")] = 0;
if (fgets(lineBuffer, sizeof(lineBuffer), infile) == NULL) {
break; }
playerDataT newPlayer = (playerDataT)malloc(sizeof(*newPlayer));
if (newPlayer == NULL) {
fprintf(stderr, "Error: Out of memory for playerDataT\n");
exit(EXIT_FAILURE);}
newPlayer->gamesPlayed = EmptyList();
char *linePtr = lineBuffer;
int offset;
int game;
if (sscanf(linePtr, "%d%n", &newPlayer->form, &offset) < 1) {
newPlayer->form = 0;}
linePtr += offset;
while (sscanf(linePtr, "%d%n", &game, &offset) == 1) {
newPlayer->gamesPlayed = Cons(game, newPlayer->gamesPlayed);
linePtr += offset; }
if (newPlayer->form >= 1 && newPlayer->form <= 3) {
newPlayer->p = prizeFunc123;
} else if (newPlayer->form >= 4 && newPlayer->form <= 5) {
newPlayer->p = prizeFunc45;} 
else if (newPlayer->form == 6) {
newPlayer->p = prizeFunc6;} 
else {newPlayer->p = prizeFunc123;}
Enter(playerTable, nameBuffer, newPlayer);}
fclose(infile);
forEachEntryDo(printPlayerPrize, playerTable);
forEachEntryDo(freePlayerData, playerTable);
free(playerTable);
return 0;}