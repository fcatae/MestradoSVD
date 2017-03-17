//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace ExtractorNet
//{
//    public class VectorInitial
//    {
//        public static WordDictionary2 GlobalUseDictionary = null;

//        Dictionary<string, int> m_dict;
//        double[] m_wordFreq;
//        int m_dictCount;

//        public VectorInitial(double[] vect, VectorInitial oldVector)
//        {
//            if (oldVector != null)
//            {
//                m_dict = oldVector.m_dict;
//                m_dictCount = oldVector.m_dictCount;
//            }

//            m_wordFreq = vect;
//        }

//        public VectorInitial(Dictionary<string, int> dict)
//        {
//            m_dict = dict;
//            m_wordFreq = new double[VectorInitial.GlobalUseDictionary.TotalCount];
//            m_dictCount = 0;

//            foreach (KeyValuePair<string, int> pair in m_dict)
//            {
//                string word = pair.Key;
//                int occur = pair.Value;

//                int wordid = GlobalUseDictionary.GetWordId(word);

//                if (wordid != -1)
//                {
//                    // Use count
//                    m_wordFreq[wordid] = occur;

//                    if (SmartCategory.GlobalUseTfIdf)
//                    {
//                        // Use tf-idf
//                        double tf = ((double)occur) / dict.Count;
//                        double idf = GlobalUseDictionary.GetIDF(word);

//                        m_wordFreq[wordid] = tf * idf;
//                    }
//                }

//                m_dictCount++;
//            }
//        }

//        public double[] GetVector()
//        {
//            return m_wordFreq;
//        }

//        public double GetValue(int position)
//        {
//            return m_wordFreq[position];
//        }

//        public double Project(DataDocument doc2)
//        {
//            Dictionary<string, int> dict1 = m_dict;
//            Dictionary<string, int> dict2 = ((VectorInitial)doc2.CachedVector).m_dict;

//            double[] vect1 = m_wordFreq;
//            double[] vect2 = doc2.CachedVector.m_wordFreq;

//            if (SmartCategory.GlobalUseDictionary)
//            {
//                double sc1 = GetScore(dict1, dict2);

//                // double sc2 = GetScore(vect1, vect2);

//                // using a lower dimension
//                //double err = sc1 - sc2;
//                //double err2 = err * err;

//                //Debug.Assert(err2 < 0.01);

//                return sc1;
//            }

//            double sc3 = GetScore(vect1, vect2);

//            return sc3;
//        }

//        public static double GetScore(Dictionary<string, int> dict1, Dictionary<string, int> dict2)
//        {
//            double score = 0;

//            foreach (string input in dict1.Keys)
//            {
//                int occur;

//                dict2.TryGetValue(input, out occur);

//                if (occur > 0)
//                    score += occur;

//                // Count all occurrences

//                // Count just one occurrence
//                //score += (occur > 0) ? 1 : 0;
//            }

//            return score / (dict1.Count + dict2.Count);
//        }

//        public static double GetScore(double[] vect1, double[] vect2)
//        {
//            double score = 0;

//            Debug.Assert(vect1.Length == vect2.Length);

//            int total = vect1.Length;

//            for(int i=0; i<total; i++)
//            {
//                score += vect1[i] * vect2[i];

//                //score += ((vect1[i] > 0) ? 1 : 0) * vect2[i];

//                //if (vect1[i] > 0)
//                //{
//                //    double occur = vect2[i];

//                //    if (occur > 0)
//                //        score += occur;

//                //    // Count all occurrences

//                //    // Count just one occurrence
//                //    //score += (occur > 0) ? 1 : 0;
//                //}
//            }

//            return score / GetNorm(vect1) / GetNorm(vect2);
//            //return score / GetNorm(vect1) / GetNorm(vect2);
//        }

//        private static double GetNorm(double[] vect1)
//        {
//            int total = vect1.Length;
//            double val = 0;

//            for (int i = 0; i < total; i++)
//            {
//                val += vect1[i] * vect1[i];
//            }

//            return Math.Sqrt(val);
//        }
//    }
//}
