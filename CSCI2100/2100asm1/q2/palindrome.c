//gcc -o palindrome palindrome.c stack.c
#include <stdio.h>
#include <string.h>
#include <ctype.h>
#include "stack.h"
int main()
{char input[81];
while(fgets(input,81,stdin)!= NULL)
{size_t len = strlen(input);
if(len>0&&input[len-1]=='\n')
{input[len-1]='\0';len--;}
if(len == 0)
{break;}
stackADT stack=EmptyStack();
for(int i=0;i<len;i++)
{Push(stack,input[i]);}
int Answer=1;
for(int i=0;i<len;i++)
{char originalChar=input[i];
char reversedChar=Pop(stack);
if(originalChar!=reversedChar)
{Answer=0;
break;
}}
if(Answer==1)
{printf("The input is a palindrome.\n");}
else
{printf("The input is NOT a palindrome.\n");}
}
return 0;
}