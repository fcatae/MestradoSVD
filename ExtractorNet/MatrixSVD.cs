using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ExtractorNet
{
    class MatrixSVD
    {
        [DllImport("Svd.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void svds(int nrow, int ncol, int nval, double[] val, int[] row, int[] offset,
                int dim, double[] Uval, double[] Sval, double[] Vval);
        [DllImport("Svd.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void svds_iteration(int nrow, int ncol, int nval, double[] val, int[] row, int[] offset,
                int dim, double[] Uval, double[] Sval, double[] Vval, int iterations);

        class VectorDiff
        {
            public readonly Document Document;
            public readonly double[] Values;
            private readonly double Module;

            public double Error
            {
                get
                {
                    int dimension = Values.Length;
                    double[] v1 = Values;
                    double[] v2 = Document.Vector.Values;

                    double error = 0;
                    double diff;

                    for (int i = 0; i < dimension; i++)
                    {
                        diff = (v1[i] - v2[i]);
                        error += diff * diff;
                    }

                    return Math.Sqrt(error) / this.Module;
                }
            }

            private VectorDiff(Document document, double[] values)
            {
                Document = document;
                Values = values;
                Module = Vector.GetModule(Document.Vector);

                Debug.Assert( Module > 0.0 );
            }

            public static double CalculateSquareError(VectorDiff[] vectorDiffs)
            {
                double error = 0;

                foreach (VectorDiff diff in vectorDiffs)
                {
                    error += diff.Error;
                }

                return error/vectorDiffs.Length;
            }

            public static VectorDiff[] CreateFrom(Document[] documents)
            {
                Debug.Assert( documents[0] != null );
                Debug.Assert( documents[0].Vector != null );
                Debug.Assert( documents[0].Vector.Values != null );
                Debug.Assert( documents[0].Vector.Values.Length > 0 );

                int total_documents = documents.Length;
                int dimension = documents[0].Vector.Values.Length;
                int count_vectors = 0;

                VectorDiff[] vectorDiffds = new VectorDiff[total_documents];

                foreach (Document doc in documents)
                {
                    Debug.Assert(doc.Vector != null);
                    Debug.Assert(doc.Vector.Values != null);
                    Debug.Assert(doc.Vector.Values.Length == dimension);

                    double[] source = doc.Vector.Values;
                    double[] destination = new double[dimension];

                    Array.Copy(source, destination, dimension);

                    vectorDiffds[count_vectors] = new VectorDiff(doc, destination);
                        
                    count_vectors++;
                }

                return vectorDiffds;
            }

        }

        class SparseMatrix
        {
            int m_totalWords;
            int m_totalDocs;
            int[] m_offset;
            int[] m_rowind;
            double[] m_values;
            int m_position;

            public int[] Offset
            {
                get { return m_offset; }
            }

            public int[] RowInd
            {
                get { return m_rowind; }
            }

            public double[] Values
            {
                get { return m_values; }
            }

            public int TotalWords
            {
                get { return m_totalWords; }
            }

            public int TotalDocs
            {
                get { return m_totalDocs; }
            }

            public int Position
            {
                get { return m_position; }
            }

            private static int RequiredValueSpace(Document[] documents)
            {
                int space = 0;

                foreach (Document doc in documents)
                {
                    Debug.Assert(doc.Vector != null);

                    foreach (double val in doc.Vector.Values)
                    {
                        if (val != 0.0)
                            space++;
                    }
                }

                return space;
            }

            public static SparseMatrix CreateFrom(Document[] documents)
            {
                SparseMatrix matrix = new SparseMatrix();

                Debug.Assert(documents != null);
                Debug.Assert( documents[0].Vector != null );

                int required_memory = RequiredValueSpace(documents);
                int total_docs = documents.Length;
                int total_words = documents[0].Vector.Values.Length;

                int[] offset = new int[total_docs + 1];
                int[] rowind = new int[required_memory];
                double[] values = new double[required_memory];

                int position = 0;

                for (int d = 0; d < total_docs; d++)
                {
                    Vector vector = documents[d].Vector;
                    
                    Debug.Assert(vector != null);
                    Debug.Assert(vector.Values.Length == total_words);

                    double[] vectorValue = vector.Values;

                    offset[d] = position;

                    for (int w = 0; w < total_words; w++)
                    {
                        double val = vectorValue[w];

                        if (val != 0.0)
                        {
                            rowind[position] = w;
                            values[position] = val;

                            position++;
                        }
                    }
                }

                offset[total_docs] = position;

                Debug.Assert(position == required_memory);

                matrix.m_offset = offset;
                matrix.m_rowind = rowind;
                matrix.m_values = values;
                matrix.m_position = position;
                matrix.m_totalWords = total_words;
                matrix.m_totalDocs = total_docs;

                return matrix;
            }


        }
        
        private Document[] m_documents;
        private VectorDiff[] m_vectorDiff;

        private int m_dimension;
        private int m_totalWords;
        private int m_totalDocs;
        private double[] m_Uval;
        private double[] m_Sval;
        private double[] m_Vval;
        private bool m_isInitialized = false;

        public int MaxDimension
        {
            get {
                Debug.Assert(m_isInitialized);

                if (!m_isInitialized)
                    throw new InvalidOperationException();

                return m_dimension; }
        }

        public MatrixSVD(DocumentGroup group)
        {
            Debug.Assert(group != null);

            m_documents = group.GetDocuments();
        }

        void PrintMatrix(Document[] documents)
        {
            int cWords = documents[0].Vector.Values.Length;
            int cDocs = documents.Length;

            Debug.WriteLine("Matrix Output");
            for(int w=0; w<cWords; w++)
            {
                for(int d=0; d<cDocs; d++)
                {
                    double[] vector = documents[d].Vector.Values;
                    string value = vector[w].ToString("F5").PadLeft(10);
                    Debug.Write(value);
                }

                Debug.WriteLine("");
            }

            Debug.WriteLine("");
        }

        public void Init()
        {
            int maxdimension = 0;

            Document[] documents = m_documents;

            SparseMatrix matrix = SparseMatrix.CreateFrom(documents);

            // calculate the SVD
            int dimension = maxdimension;
            int total_docs = matrix.TotalDocs;
            int total_word = matrix.TotalWords;

            if (dimension == 0)             dimension = total_docs;
            if (dimension > total_docs)     dimension = total_docs;
            if (dimension > total_word)     dimension = total_word;

            m_dimension = dimension;

            if (dimension == 1)
            {
                double module = Vector.GetModule(documents[0].Vector);

                m_Uval = new double[total_word];
                m_Sval = new double[1];
                m_Vval = new double[total_docs];

                //Array.Copy(documents[0].Vector.Values, m_Uval, total_word);
                for (int i = 0; i < total_word; i++)
                    m_Uval[i] = documents[0].Vector.Values[i] / module;

                m_Sval[0] = 1;
                m_Vval[0] = 1;
            }

            // Calculate the SVD
            Svd(matrix);

            m_vectorDiff = VectorDiff.CreateFrom(documents);
            
            m_isInitialized = true;
        }

        private void Svd(SparseMatrix matrix)
        {
            Debug.Assert(m_dimension > 0);
            Debug.Assert(matrix.TotalWords > 0);
            Debug.Assert(matrix.TotalDocs > 0);
            Debug.Assert(matrix.Position > 0);
            Debug.Assert(matrix.Values != null);
            Debug.Assert(matrix.RowInd != null);
            Debug.Assert(matrix.Offset != null);

            int dimension = m_dimension;
            
            double[] Uval = new double[matrix.TotalWords * dimension];
            double[] Sval = new double[dimension];
            double[] Vval = new double[matrix.TotalDocs * dimension];

            m_totalWords = matrix.TotalWords;
            m_totalDocs = matrix.TotalDocs;

            if (dimension == 1)
            {
                return;
            }

            svds_iteration(matrix.TotalWords, matrix.TotalDocs, matrix.Position,
                matrix.Values, matrix.RowInd, matrix.Offset,
                dimension
                , Uval, Sval, Vval,5);

            m_Uval = Uval;
            m_Sval = Sval;
            m_Vval = Vval;

        }

        private void Reconstruct(int approximation)
        {
            Debug.Assert(m_isInitialized);
            Debug.Assert(approximation <= m_dimension);

            Document[] documents = m_documents;
            int total_docs = m_totalDocs;
            int total_word = m_totalWords;

            double[] Uval = m_Uval;
            double[] Sval = m_Sval;
            double[] Vval = m_Vval;

            int dimension = m_dimension;

            if (approximation == 0)
                approximation = m_dimension;

            for (int d = 0; d < total_docs; d++)
            {
                double[] vector = documents[d].Vector.Values;

                for (int w = 0; w < total_word; w++)
                {
                    double val = 0;

                    for (int col = 0; col < approximation; col++)
                    {
                        double u1 = Uval[w * dimension + col];
                        double s1 = Sval[col];
                        double v1 = Vval[d * dimension + col];

                        val += v1 * s1 * u1;
                    }

                    vector[w] = val;
                }
            }
        }

        private void CreateSvdVectors()
        {
            Document[] documents = m_documents;
            int total_docs = m_totalDocs;

            for (int d = 0; d < total_docs; d++)
            {
                Vector vector = documents[d].Vector;

                if (!(vector is SvdVector))
                    documents[d].Set( new SvdVector(vector) );
            }

        }

        public void SetDimension(int approximation)
        {
            Debug.Assert(m_isInitialized);
            Debug.Assert(approximation > 0);

            if (!m_isInitialized)
                throw new InvalidOperationException();

            if (approximation > m_dimension)
                approximation = m_dimension;

            Reconstruct(approximation);
            CreateSvdVectors();
        }

        public double GetApproximationError()
        {
            Debug.Assert(m_isInitialized);

            if (!m_isInitialized)
                throw new InvalidOperationException();
            
            double error = VectorDiff.CalculateSquareError(m_vectorDiff);

            return error;
        }
        
        public void GetBaseVectors(int approximation)
        {
            Debug.Assert(m_isInitialized);
            Debug.Assert(approximation > 0);

            if (!m_isInitialized)
                throw new InvalidOperationException();

            if (approximation > m_dimension)
                approximation = m_dimension;

            Document[] documents = m_documents;
            int total_docs = m_totalDocs;
            int total_word = m_totalWords;

            double[] Uval = m_Uval;
            double[] Sval = m_Sval;
            double[] Vval = m_Vval;

            int dimension = m_dimension;

            if (approximation == 0)
                approximation = m_dimension;

            double Sconst = 0.0;
            for (int col = 0; col < approximation; col++)
                Sconst += Sval[col] * Sval[col];
            Sconst = Math.Sqrt(Sconst);
            
            for (int d = 0; d < approximation; d++)
            {
                double[] vector = documents[d].Vector.Values;

                for (int w = 0; w < total_word; w++)
                {
                    //double val = 0;

                    //for (int col = 0; col < approximation; col++)
                    //{
                    //    double u1 = Uval[w * dimension + col];
                    //    double s1 = Sval[col];
                    //    double v1 = Vval[d * dimension + col];

                    //    val += u1/Math.Sqrt(s1);// *v1*s1
                    //}

                    //vector[w] = (w == d) ? 1 : 0;

                    vector[w] = Uval[w * dimension + d];
                    //vector[w] = Uval[d * total_word + w];
                    //vector[w] = Vval[w * dimension + d];
                    //vector[w] = Vval[d * dimension + w];

                    //vector[w] = Uval[w * dimension + d];
                    //vector[w] = Vval[d * dimension + w];
                    //vector[w] = Vval[d * dimension + w];// *Sval[d];
                    //vector[w] = Uval[w * dimension + d] * Sval[d];
                    //vector[w] = Vval[d * dimension + w] * Sval[d];
                    //vector[w] = Uval[w * dimension + d] / Sval[d];
                    //vector[w] = Uval[w * dimension + d] * Math.Sqrt(Sval[d]);

                    // Combinacao 1
                    //vector[w] = Uval[w * dimension + d] * Sval[d];
                    //vector[w] /= Sconst; 

                    // Combinacao 2
                    //vector[w] = Uval[w * dimension + d] * Sval[d];
                    //documents[d].Vector.Normalize();

                    //vector[w] /= Sconst;
                }


                //documents[d].Vector.Normalize();
            }

            for (int d = approximation; d < total_docs; d++)
            {
                double[] vector = documents[d].Vector.Values;

                for (int w = 0; w < total_word; w++)
                    vector[w] = 0;
            }

            // vetores ortogonais e unitários
            #if DEBUG
            for (int i = 0; i < documents.Length - 1; i++)
            {
                Vector v1 = documents[i].Vector;
                Vector v2 = documents[i + 1].Vector;

                double ang = Math.Abs(Vector.Dot(v1, v2));
                double m1 = Vector.GetModule(v1);
                double m2 = Vector.GetModule(v2);

                Debug.Assert((Math.Abs(m1 - 1.0) < 1e-5) || (Math.Abs(m1 - 0.0) < 1e-5));
                Debug.Assert((Math.Abs(m2 - 1.0) < 1e-5) || (Math.Abs(m2 - 0.0) < 1e-5));
                
                Debug.Assert((Double.IsNaN(ang)) || (ang < .001));
            }
            #endif
        }
    }
}
