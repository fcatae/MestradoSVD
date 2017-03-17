//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Runtime.InteropServices;

//namespace ExtractorNet
//{
//    class SmartCategory : SmartDude
//    {
//        enum EnumClassificationMethod { Nearest, AvgScore, Centroid };
        
//        private static EnumClassificationMethod Config_ClassificationMethod = EnumClassificationMethod.Nearest;
//        private static int Config_MinimumWordSize = 0;
////        private static bool Config_UseTfIdf = true;
//        private static int Config_MinOccur = 10;
//        public static bool GlobalUseTfIdf = true;
//        public static bool GlobalUseDictionary = true;
//        public static bool GlobalUseSVD = true;
//        public static int GlobalUseDimensions = 200;

//        [DllImport("Svd.dll", CallingConvention = CallingConvention.Cdecl)]
//        public static extern void svds(int nrow, int ncol, int nval, double[] val, int[] row, int[] offset,
//                int dim, double[] Uval, double[] Sval, double[] Vval);


//        public class SGroup
//        {
//            private static Regex sm_regex = new Regex(@"\w+", RegexOptions.Compiled);

//            string m_name;
//            List<DataDocument> m_group = new List<DataDocument>();
//            public SGroup(string name)
//            {
//                m_name = name;
//            }

//            public string Name
//            {
//                get { return m_name; }
//            }

//            public int Count
//            {
//                get { return m_group.Count; }
//            }

//            public void Add(DataDocument doc)
//            {
//                m_group.Add(doc);
//            }

//            public double GetScore(DataDocument doc)
//            {
//                return GetScore(0, doc);
//            }

//            public double GetScore(double min_score, DataDocument doc)
//            {
//                double score = 0;
//                int total = m_group.Count;

//                double maxscore = min_score;

//                if (Config_ClassificationMethod == EnumClassificationMethod.Centroid)
//                {
//                    int size = m_group[0].CachedVector.GetVector().Length;
//                    double[] vector = new double[size];

//                    for (int i = 0; i < total; i++)
//                    {
//                        DataDocument currentDoc = m_group[i];

//                        double[] v = currentDoc.CachedVector.GetVector();

//                        for (int j = 0; j < size; j++)
//                            vector[j] += v[j];
//                    }

//                    VectorInitial newvect = new VectorInitial(vector, null);

//                    return newvect.Project(doc) / total;
//                }

//                // Count the number of similar words
//                for (int i = 0; i < total; i++)
//                {
//                    DataDocument currentDoc = m_group[i];

//                    double similarity = GetScore(m_group[i], doc);

//                    score += similarity;

//                    if (similarity >= maxscore)
//                    {
//                        maxscore = similarity;
//                    }
//                }

//                double total_score = score / total;

//                if (Config_ClassificationMethod == EnumClassificationMethod.AvgScore)
//                    return total_score;

//                if( Config_ClassificationMethod == EnumClassificationMethod.Nearest)
//                    return maxscore;

//                Debug.Assert(false);

//                return 0;
//            }

//            static double GetScore(DataDocument doc1, DataDocument doc2)
//            {

//                if (doc1 == doc2)
//                    return 0;

//                CreateCachedDictionary(doc1);
//                CreateCachedDictionary(doc2);

//                return doc1.CachedVector.Project(doc2);
                
//                //return Vector.GetScore(dict1, dict2);               
//            }

//            public static void CreateCachedDictionary(DataDocument doc1)
//            {
//                Dictionary<string, int> dict1 = (doc1.CachedObj == null) ? CreateDictionary(doc1) : (Dictionary<string, int>)doc1.CachedObj;

//                doc1.CachedObj = dict1;

//                if (doc1.CachedVector == null)
//                {
//                    if (VectorInitial.GlobalUseDictionary != null)
//                        doc1.CachedVector = new VectorInitial(dict1);
//                }
//            }

//            //public static void CreateCachedVector(Doc doc1)
//            //{
//            //    if (doc1.CachedObj == null)
//            //    {
//            //        Dictionary<string, int> dict1 = CreateDictionary(doc1);

//            //        doc1.CachedObj = new Vector(dict1);
//            //    }
//            //}

//            static Dictionary<string, int> CreateDictionary(DataDocument doc)
//            {
//                Dictionary<string, int> dict = new Dictionary<string, int>();
//                string text = doc.Text;

//                MatchCollection collection = sm_regex.Matches(text);
//                //text.Split(,,StringSplitOptions.RemoveEmptyEntries);

//                foreach (Match match in collection)
//                {
//                    string val = match.Value.ToLower();

//                    // Config: ignore small words
//                    if (Config_MinimumWordSize > 0)
//                    {
//                        if (val.Length <= Config_MinimumWordSize)
//                            continue;
//                    }

//                    if (dict.ContainsKey(val))
//                        dict[val]++;
//                    else
//                        dict.Add(val, 1);
//                }

//                return dict;
//            }
//        }

//        private Dictionary<string, SGroup> m_categoryGroup = new Dictionary<string, SGroup>();

//        public void Prepare(ICollection<DataDocument> training, ICollection<DataDocument> testset)
//        {
//            WordDictionary2 wordDict = new WordDictionary2();

//            ICollection<DataDocument>[] all_docs = new ICollection<DataDocument>[] { training, testset };

//            // define the dictionary
//            foreach (ICollection<DataDocument> collection in all_docs)
//            {
//                foreach (DataDocument doc in collection)
//                {
//                    wordDict.RegisterDocument();

//                    SGroup.CreateCachedDictionary(doc);

//                    Dictionary<string, int> words = (Dictionary<string, int>)doc.CachedObj;

//                    foreach (string w in words.Keys)
//                    {
//                        wordDict.Add(w);
//                    }
//                }

//            }
            
//            wordDict.ClearnupUnusedValues(Config_MinOccur);
//            VectorInitial.GlobalUseDictionary = wordDict;  
          
//            // create the cached vector
//            foreach (ICollection<DataDocument> collection in all_docs)
//            {
//                foreach (DataDocument doc in collection)
//                {
//                    SGroup.CreateCachedDictionary(doc);
//                }
//            }

//            if( GlobalUseSVD)
//            {
//                List<DataDocument> allDocs = new List<DataDocument>();
//                allDocs.AddRange(training);
//                allDocs.AddRange(testset);

//                // calculate the SVD
//                int total_word = wordDict.TotalCount;
//                int total_docs = allDocs.Count;
//                int dimension = (GlobalUseDimensions > 0) ? GlobalUseDimensions : allDocs.Count;

//                int CHUTE = wordDict.WordDocCount;

//                int[] offset = new int[total_word];

//                int[] rowind = new int[CHUTE];
//                double[] values = new double[CHUTE];

//                int position = 0;

//                //for (int w = 0; w < total_word; w++)
//                //{
//                //    offset[w] = position;

//                //    for (int d = 0; d < total_docs; d++ )
//                //    {
//                //        double val = allDocs[d].CachedVector.GetValue(w);

//                //        if (val != 0.0)
//                //        {
//                //            rowind[position] = d;
//                //            values[position] = val;

//                //            position++;
//                //        }
//                //    }
//                //}

//                for (int d = 0; d < total_docs; d++)
//                {
//                    offset[d] = position;

//                    for (int w = 0; w < total_word; w++)
//                    {
//                        double val = allDocs[d].CachedVector.GetValue(w);

//                        if (val != 0.0)
//                        {
//                            rowind[position] = w;
//                            values[position] = val;

//                            position++;
//                        }
//                    }
//                }

//                offset[total_docs] = position;

//                // Calculate the SVD
//                double[] Uval = new double[total_word * dimension];
//                double[] Sval = new double[dimension];
//                double[] Vval = new double[total_docs * dimension];

//                svds(total_word, total_docs, position, values, rowind, offset, dimension, Uval, Sval, Vval);

//                //double Sval_tot = 0;
//                //for (int i = 0; i < dimension; i++)
//                //    Sval_tot += Sval[i];

//                //double[] Sval2 = new double[dimension];
//                //for (int i = 0; i < dimension; i++)
//                //    Sval2[i] = Sval[i] / Sval_tot;

//                //double[] Sval3 = new double[dimension];
//                //Sval3[0] = Sval2[0];
//                //for (int i = 1; i < dimension; i++)
//                //    Sval3[i] = Sval3[i - 1] + Sval2[i];

//                // Reconstruir a matriz
//                // U x S x V


//                for (int d = 0; d < total_docs; d++)
//                {
//                    double[] dblVector = new double[total_word];

//                    for (int w = 0; w < total_word; w++)
//                    {
//                        double val = 0;

//                        for (int col = 0; col < dimension; col++)
//                        {
//                            double u1 = Uval[w * dimension + col];
//                            double s1 = Sval[col];
//                            double v1 = Vval[d * dimension + col];

//                            val += v1 * s1 * u1;
//                        }

//                        // Validate
//                        if (dimension == total_docs)
//                        {
//                            double expected = allDocs[d].CachedVector.GetValue(w);
//                            double err = (expected - val);
//                            Debug.Assert(err * err < 0.01);
//                        }

//                        dblVector[w] = val;
//                    }

//                    VectorInitial vector = new VectorInitial(dblVector, allDocs[d].CachedVector);

//                    allDocs[d].CachedVector = vector;
//                }
//        }

//            Console.WriteLine("Time = {0}", DateTime.Now);
//        }

//        public void Train(DataDocument doc)
//        {
//            string category = doc.Category;
//            SGroup group;

//            if (!m_categoryGroup.ContainsKey(category))
//            {
//                m_categoryGroup.Add(category, new SGroup(category));
//            }

//            group = m_categoryGroup[category];

//            group.Add(doc);
//        }

//        public string Classify(DataDocument doc)
//        {
//            double score_preview = Double.MinValue; ;
//            double maxscore = Double.MinValue;
//            string max_category = null;

//            foreach (string category in m_categoryGroup.Keys)
//            {
//                SGroup group = m_categoryGroup[category];
//                double score;

//                score = group.GetScore(doc);
                
//                if (score > maxscore)
//                {
//                    maxscore = score;
//                    max_category = group.Name;
//                }

//                if (category == doc.Category)
//                {
//                    score_preview = score;
//                }
//            }

//            Debug.Assert(max_category != null);

//            // Debugging Purpose
//            if( doc.Category != max_category )
//            {
//                //Debug.WriteLine("cat " + max_category);

//                if (m_categoryGroup.ContainsKey(doc.Category))
//                {
//                    // Replay the calculations
//                    if (Config_ClassificationMethod == EnumClassificationMethod.Nearest)
//                    {
//                        // replay the specific calculation
//                        maxscore = m_categoryGroup[max_category].GetScore(maxscore, doc);
//                        score_preview = m_categoryGroup[doc.Category].GetScore(score_preview, doc);
//                    }

//                    maxscore = m_categoryGroup[max_category].GetScore(doc);
//                    score_preview = m_categoryGroup[doc.Category].GetScore(doc);

//                }
//                else
//                {
//                    // Unknown category during the test
//                }
//            }

//            return max_category;
//        }
//    }
//}
