#include <stdio.h>
#include "prizeFnT.h"
#include "list.h"
static int isMember(listElementT elem, listADT list) {
if (ListIsEmpty(list)) {
return 0;}
if (Head(list) == elem) {
return 1;}
return isMember(elem, Tail(list));}
static int countUniqueGames(listADT gamesPlayed) {
if (ListIsEmpty(gamesPlayed)) {
return 0;}
int countInTail = countUniqueGames(Tail(gamesPlayed));
if (!isMember(Head(gamesPlayed), Tail(gamesPlayed))) {
return 1 + countInTail;
}return countInTail;}
prizeT prizeFunc123(int form, listADT gamesPlayed) {
int n = countUniqueGames(gamesPlayed);
if (n >= 4) return grandPrize;
if (n == 3) return largePrize;
if (n == 2) return mediumPrize;
if (n == 1) return smallPrize;
return noPrize;}
prizeT prizeFunc45(int form, listADT gamesPlayed) {
int n = countUniqueGames(gamesPlayed);
if (n >= 7) return grandPrize;
if (n >= 5) return largePrize;
if (n >= 3) return mediumPrize;
if (n >= 1) return smallPrize;
return noPrize;}
prizeT prizeFunc6(int form, listADT gamesPlayed) {
int n = countUniqueGames(gamesPlayed);
if (n == 10) return grandPrize;
if (n >= 7) return largePrize;
if (n >= 4) return mediumPrize;
if (n >= 1) return smallPrize;
return noPrize;}