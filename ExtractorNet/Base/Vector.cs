using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class SvdVector : Vector
    {
        public SvdVector(Vector vector) : base(vector)
        {
        }
    }

    class Vector
    {
        private WordDictionary m_wordDictionary;
        private double[] m_vector;

        public WordDictionary Dictionary
        {
            get { return m_wordDictionary; }
        }

        public double[] Values
        {
            get { return m_vector; }
        }

        protected Vector(Vector vector)
        {
            m_wordDictionary = vector.m_wordDictionary;
            m_vector = vector.m_vector;
        }

        public Vector(Bow document, WordDictionary dict)
        {
            Debug.Assert( dict.IsComplete );
            Debug.Assert( dict.TotalWords > 0 );

            m_vector = CreateVector(document, dict);
            m_wordDictionary = dict;

            Debug.Assert(m_vector.Length > 0);
        }

        public void Normalize()
        {
            double[] vector = m_vector;
            int dimension = vector.Length;
            double module = Module(vector);

            for (int i = 0; i < dimension; i++)
                vector[i] /= module;
        }

        static double Module(double[] v)
        {
            double moduleSquare = 0;
            int dimension = v.Length;

            for (int i = 0; i < dimension; i++)
                moduleSquare += v[i] * v[i];

            return Math.Sqrt(moduleSquare);
        }

        public static double GetModule(Vector vector)
        {
            return Module(vector.Values);
        }

        static double[] CreateBlankVector(WordDictionary dict)
        {
            int size = dict.TotalWords;

            return new double[size];
        }

        static double[] CreateVector(Bow document, WordDictionary dict)
        {
            double[] vector = CreateBlankVector(dict);

            foreach (string word in document.Words)
            {
                int id = dict.TryGetIdentifier(word);

                if (id != WordDictionary.InvalidIdentifier)
                {
                    int count = document.GetOccurrances(word);

                    Debug.Assert(count > 0);

                    vector[id] += count;
                }
            }

            return vector;
        }

        public static bool IsUnitOrNull(Vector vector1)
        {
            double module = GetModule(vector1);

            if (Math.Abs(module - 1.0) < 1e-5)
                return true;

            if (Math.Abs(module - 0.0) < 1e-5)
                return true;

            return false;
        }
        
        public static double Dot(Vector vector1, Vector vector2)
        {
            Debug.Assert(vector1.m_wordDictionary == vector2.m_wordDictionary);

            double[] v1 = vector1.Values;
            double[] v2 = vector2.Values;
            int dimension = v1.Length;

            Debug.Assert(v1.Length == v2.Length);

            double result = 0;

            for (int dim = 0; dim < dimension; dim++)
                result += v1[dim] * v2[dim];

            return result;
        }

        [Obsolete] // be careful - it return NAN!!!
        public static double Cosine(Vector vector1, Vector vector2)
        {
            Debug.Assert(vector1.m_wordDictionary == vector2.m_wordDictionary);

            double[] v1 = vector1.Values;
            double[] v2 = vector2.Values;
            int dimension = v1.Length;

            Debug.Assert(v1.Length == v2.Length);

            double result = 0;

            double mod1 = Module(v1);
            double mod2 = Module(v2);

            if ((mod1 == 0) || (mod2 == 0))
                return 0.0;

            for (int dim = 0; dim < dimension; dim++)
                result += v1[dim] * v2[dim];

            return result/mod1/mod2;
        }
    }


}
