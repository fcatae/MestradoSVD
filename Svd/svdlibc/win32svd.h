#include <stdio.h>

FILE* popen(char *pipeName, char *mode);
void pclose(FILE* file);

long htonl(long hostlong);
long ntohl(long hostlong);