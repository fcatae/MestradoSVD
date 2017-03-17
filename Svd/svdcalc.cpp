// SvdCalc.cpp : Defines the entry point for the console application.
//
#include <assert.h>
#include <stdio.h>
#include <cstring>

#include "svdlibc\svdlib.h"

//extern "C" __declspec(dllexport)
//
//void Teste()
//{
//	printf("SvdCalc\n");
//
//	struct smat* idfTransposed = NULL;
//
//	struct smat* mat = svdNewSMat(2, 3, 6);
//	
//	int val=0;
//	mat->pointr[0] = val;
//
//	mat->rowind[val] = 0;
//	mat->value[val] = 1;
//	val++;
//
//	mat->rowind[val] = 1;
//	mat->value[val] = 4;
//	val++;
//
//	mat->pointr[1] = val;
//
//	mat->rowind[val] = 0;
//	mat->value[val] = 2;
//	val++;
//
//	mat->rowind[val] = 1;
//	mat->value[val] = 5;
//	val++;
//	
//	mat->pointr[2] = val;
//
//	mat->rowind[val] = 0;
//	mat->value[val] = 3;
//	val++;
//
//	mat->rowind[val] = 1;
//	mat->value[val] = 6;
//	val++;
//
//	mat->pointr[3] = 6;
//	
//	struct svdrec* res = svdLAS2A(mat, 3);
//
//	int dim = 3;
//	double Sval[3];
//	double Uval[9];
//	double Vval[9];
//
//	//svdFreeSMat(idfTransposed);
//	DMat U, V; double *S;
//	S = res->S;
//	memcpy(Sval, S, sizeof(double) * dim);
//
//	U = svdTransposeD(res->Ut);
//	V = svdTransposeD(res->Vt);
//	memcpy(Uval, &U->value[0][0], sizeof(double) * (U->rows * U->cols));
//	memcpy(Vval, &V->value[0][0], sizeof(double) * (V->rows * U->cols));
//
//}

extern "C" __declspec(dllexport)
void svds(int nrow, int ncol, int nval, double *val, int *row, int *offset,
		int dim, double Uval[], double Sval[], double Vval[])
{
	assert( nrow > 0 );
	assert( ncol > 0 );
	assert( nval > 0 );
	assert( dim > 0 );

	SMat A;
	SVDRec res;
	DMat U, V; 
	double *S;
	FILE *fp;

	A = svdNewSMat(nrow, ncol, nval);
	
	memcpy(A->value, val, sizeof(double) * nval);
	memcpy(A->rowind, row, sizeof(int) * nval);
	memcpy(A->pointr, offset, sizeof(int) * (ncol+1));
	
	res = svdLAS2A(A, dim);
	
	S = res->S;memcpy(Sval, S, sizeof(double) * dim);

	U = svdTransposeD(res->Ut);	
	V = svdTransposeD(res->Vt);
	
	memcpy(Uval, &U->value[0][0], sizeof(double) * (U->rows * U->cols));
	memcpy(Vval, &V->value[0][0], sizeof(double) * (V->rows * V->cols));
	
	assert(	U->cols == dim );
	assert(	U->rows == nrow );
	assert(	V->cols == dim );
	assert(	V->rows == ncol );

	svdFreeDMat(U);
	svdFreeDMat(V);
	svdFreeSMat(A);
	
	svdFreeSVDRec(res);
}

extern "C" __declspec(dllexport)
void svds_iteration(int nrow, int ncol, int nval, double *val, int *row, int *offset,
		int dim, double Uval[], double Sval[], double Vval[], int iterations)
{
	assert( nrow > 0 );
	assert( ncol > 0 );
	assert( nval > 0 );
	assert( dim > 0 );

	SMat A;
	SVDRec res;
	DMat U, V; 
	double *S;
	FILE *fp;

	A = svdNewSMat(nrow, ncol, nval);
	
	memcpy(A->value, val, sizeof(double) * nval);
	memcpy(A->rowind, row, sizeof(int) * nval);
	memcpy(A->pointr, offset, sizeof(int) * (ncol+1));
	
	res = svdLAS2A_iteration(A, dim, iterations);
	
	S = res->S;memcpy(Sval, S, sizeof(double) * dim);

	U = svdTransposeD(res->Ut);	
	V = svdTransposeD(res->Vt);
	
	memcpy(Uval, &U->value[0][0], sizeof(double) * (U->rows * U->cols));
	memcpy(Vval, &V->value[0][0], sizeof(double) * (V->rows * V->cols));
	
	assert(	U->cols == dim );
	assert(	U->rows == nrow );
	assert(	V->cols == dim );
	assert(	V->rows == ncol );

	svdFreeDMat(U);
	svdFreeDMat(V);
	svdFreeSMat(A);
	
	svdFreeSVDRec(res);
}