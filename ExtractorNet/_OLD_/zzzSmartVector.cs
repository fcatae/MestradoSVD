//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace ExtractorNet
//{
//    [Obsolete]
//    class SmartVector : SmartDude
//    {
//        class SGroup
//        {
//            private static Regex sm_regex = new Regex(@"\w+", RegexOptions.Compiled);

//            string m_name;
//            List<CatDocument> m_group = new List<CatDocument>();
//            public SGroup(string name)
//            {
//                m_name = name;
//            }

//            public string Name
//            {
//                get { return m_name; }
//            }

//            public void Add(CatDocument doc)
//            {
//                m_group.Add(doc);
//            }

//            public double GetScore(CatDocument doc)
//            {
//                double score = 0;
//                int total = m_group.Count;

//                double maxscore = 0;

//                // Count the number of similar words
//                for (int i = 0; i < total; i++)
//                {
//                    CatDocument currentDoc = m_group[i];

//                    double similarity = GetScore(m_group[i], doc);

//                    score += similarity;

//                    if (similarity > maxscore)
//                    {
//                        maxscore = similarity;
//                    }
//                }

//                double total_score = score / total;

//                //return total_score;

//                return maxscore;
//            }

//            static double GetScore(CatDocument doc1, CatDocument doc2)
//            {
//                double score = 0;

//                if (doc1 == doc2)
//                    return 0;

//                Dictionary<string, int> dict1 = (doc1.CachedObj == null) ? CreateDictionary(doc1) : (Dictionary<string, int>)doc1.CachedObj;
//                Dictionary<string, int> dict2 = (doc2.CachedObj == null) ? CreateDictionary(doc2) : (Dictionary<string, int>)doc2.CachedObj;

//                doc1.CachedObj = dict1;
//                doc2.CachedObj = dict2;

//                foreach (string input in dict1.Keys)
//                {
//                    int occur;

//                    dict2.TryGetValue(input, out occur);

//                    // Count all occurrences
//                    score += occur;

//                    // Count just one occurrence
//                    //score += (occur > 0) ? 1 : 0;
//                }

//                return score / dict2.Count;
//            }

//            static Dictionary<string, int> CreateDictionary(CatDocument doc)
//            {
//                Dictionary<string, int> dict = new Dictionary<string, int>();
//                string text = doc.Text;

//                MatchCollection collection = sm_regex.Matches(text);
//                //text.Split(,,StringSplitOptions.RemoveEmptyEntries);

//                foreach (Match match in collection)
//                {
//                    string val = match.Value.ToLower();

//                    // ignore small words
//                    if (val.Length <= 3)
//                        continue;

//                    if (dict.ContainsKey(val))
//                        dict[val]++;
//                    else
//                        dict.Add(val, 1);
//                }

//                return dict;
//            }
//        }

//        private Dictionary<string, SGroup> m_categoryGroup = new Dictionary<string, SGroup>();

//        public void Prepare(ICollection<CatDocument> training, ICollection<CatDocument> testset)
//        {
//        }

//        public void Train(CatDocument doc)
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

//        public string Classify(CatDocument doc)
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

//            //Debug.WriteLine(max_category);

//            if (doc.Category != max_category)
//            {
//                if (m_categoryGroup.ContainsKey(doc.Category))
//                {
//                    // Replay the calculations
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
