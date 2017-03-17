using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections;

namespace ExtractorNet
{
    class WordDictionary
    {
        public const int InvalidIdentifier = -1;

        private Dictionary<string, int> m_wordIndex = new Dictionary<string, int>();
        private bool m_complete = false;

        public int TotalWords
        {
            get
            {
                Debug.Assert(m_complete);
                return m_wordIndex.Count; }
        }

        public bool IsComplete
        {
            get {return m_complete; }
        }

        public virtual int TryGetIdentifier(string word)
        {
            Debug.Assert(m_complete);

            if (m_wordIndex.ContainsKey(word))
                return m_wordIndex[word];

            return InvalidIdentifier;
        }

        protected int InternalGetWordIndex(string word)
        {
            if (m_wordIndex.ContainsKey(word))
                return m_wordIndex[word];

            return InvalidIdentifier;
        }

        public virtual void AddBow(Bow bow)
        {
            Debug.Assert(!m_complete);

            foreach (string word in bow.Words)
            {
                int occurrences = bow.GetOccurrances(word);

                UpdateWordCount(word, occurrences);
            }
        }

        public virtual void Complete()
        {
            Debug.Assert(!m_complete);
            Debug.Assert(m_wordIndex.Count > 0);

            m_complete = true;
        }

        protected virtual void UpdateWordCount(string word, int count)
        {
            if (!m_wordIndex.ContainsKey(word))
            {
                int index = m_wordIndex.Count;

                m_wordIndex.Add(word, index);
            }
        }
    }

    class WordDictionaryStat : WordDictionary
    {
        public class PairWordCount
        {
            public PairWordCount(string word, int count, int perdocument)
            {
                this.Word = word;
                this.Count = count;
                this.CountPerDocument = perdocument;
            }

            public readonly string Word;
            public int Count;
            public int CountPerDocument;
        }

        protected List<PairWordCount> m_statCount = new List<PairWordCount>();
        protected int m_statDocCount = 0;

        public override void AddBow(Bow bow)
        {
            base.AddBow(bow);

            m_statDocCount++;
        }
        
        protected override void UpdateWordCount(string word, int count)
        {
            int index = InternalGetWordIndex(word);

            if( index == InvalidIdentifier )
            {
                m_statCount.Add(new PairWordCount(word, count, 1));
            }
            else
            {
                m_statCount[index].Count += count;
                m_statCount[index].CountPerDocument += 1;
            }

            base.UpdateWordCount(word, count);
        }

        public IEnumerable<PairWordCount> GetStatCount()
        {
            return m_statCount;
        }
    }

}
