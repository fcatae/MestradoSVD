//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace ExtractorNet
//{
//    public class WordDictionary2
//    {
//        private List<string> m_wordList = new List<string>();
//        private List<int> m_wordCountList = new List<int>();
//        private Dictionary<string, int> m_wordIndex = new Dictionary<string, int>();
//        private int m_totalWordDoc = 0;

//        [Obsolete]
//        private int m_totalDocs = 0;

//        [Obsolete]
//        public int WordDocCount
//        {
//            get { return m_totalWordDoc; }
//        }

//        [Obsolete]
//        public void RegisterDocument()
//        {
//            m_totalDocs++;
//        }

//        public void ClearnupUnusedValues(int min_occur)
//        {
//            List<string> wordList = new List<string>();
//            List<int> wordCountList = new List<int>();
//            Dictionary<string, int> wordIndex = new Dictionary<string,int>();

//            foreach (string word in m_wordList)
//            {
//                int index = m_wordIndex[word];
//                int occur = m_wordCountList[index];
//                int position = wordList.Count;

//                Debug.Assert(wordList.Count == wordCountList.Count);

//                if (occur < min_occur)
//                    continue;

//                wordList.Add(word);
//                wordCountList.Add(occur);


//                wordIndex.Add(word, position);
//            }

//            m_wordList = wordList;
//            m_wordCountList = wordCountList;
//            m_wordIndex = wordIndex;
//        }

//        public int TotalCount
//        {
//            get { return m_wordList.Count; }
//        }

//        public int GetWordId(string word)
//        {
//            if( m_wordIndex.ContainsKey(word))
//                return m_wordIndex[word];

//            return -1;
//        }

//        public void Add(string word)
//        {
//            m_totalWordDoc++;

//            if( !m_wordIndex.ContainsKey(word) )
//            {
//                m_wordList.Add(word);
                
//                int index = m_wordList.Count - 1;

//                m_wordIndex[word] = index;
//                m_wordCountList.Add(1);                
//            }
//            else
//            {
//                int index = m_wordIndex[word];
//                m_wordCountList[index]++;
//            }            
//        }

//        public double GetIDF(string word)
//        {
//            int index = m_wordIndex[word];

//            int docCount = m_wordCountList[index];
//            int totalCount = m_totalDocs;

//            return Math.Log(((double)totalCount) / docCount);
//        }
//    }
//}
