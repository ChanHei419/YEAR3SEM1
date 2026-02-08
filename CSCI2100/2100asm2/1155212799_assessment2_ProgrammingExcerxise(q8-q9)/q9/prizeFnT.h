#ifndef PRIZEFNT_H
#define PRIZEFNT_H
#include "list.h"
typedef enum {grandPrize, largePrize, mediumPrize, smallPrize, noPrize} prizeT;
typedef prizeT (*prizeFnT)(int, listADT);
prizeT prizeFunc123(int form, listADT gamesPlayed);
prizeT prizeFunc45(int form, listADT gamesPlayed);
prizeT prizeFunc6(int form, listADT gamesPlayed);
#endif