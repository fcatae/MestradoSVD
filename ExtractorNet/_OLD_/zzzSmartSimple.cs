//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ExtractorNet
//{
//    // Return the most-frequent category
//    [Obsolete]
//    class SmartSimple : SmartDude
//    {
//        public Dictionary<string, int> m_freqCategory = new Dictionary<string, int>();
//        private string m_lastCategory = null;

//        public void Train(CatDocument doc)
//        {
//            if (!m_freqCategory.ContainsKey(doc.Category))
//            {
//                m_freqCategory.Add(doc.Category, 0);
//            }

//            m_freqCategory[doc.Category]++;
//        }

//        public void Prepare(ICollection<CatDocument> training, ICollection<CatDocument> testset)
//        {
//        }

//        public string Classify(CatDocument doc)
//        {
//            if (m_lastCategory == null)
//            {
//                int max = 0;
//                foreach (KeyValuePair<string, int> pair in m_freqCategory)
//                {
//                    if (pair.Value > max)
//                    {
//                        m_lastCategory = pair.Key;
//                        max = pair.Value;
//                    }
//                }
//            }

//            return m_lastCategory;
//        }
//    }
//}
