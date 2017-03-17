#include<assert.h>
#include<stdio.h>

#ifndef SVD_WIN32
#define SVD_WIN32

// wrong implementation - but it should do the trick
long htonl(long hostlong)
{
	return hostlong;
}

// wrong implementation - but it should do the trick
long ntohl(long hostlong)
{
	return hostlong;
}

FILE* popen(char *pipeName, char *mode)
{
	// Not implemented
	assert( "Implementation" == NULL );

	return NULL;
}

void pclose(FILE* file)
{
	// Not implemented
	assert( "Implementation" == NULL );
}


#endif