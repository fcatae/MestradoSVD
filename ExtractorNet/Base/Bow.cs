using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtractorNet
{
    class Bow
    {
        private static Regex sm_regex = new Regex(@"\w+", RegexOptions.Compiled);

        private Dictionary<string, int> m_wordIndexCount;

        public Bow(TextDocument doc)
        {
            m_wordIndexCount = CreateDictionary(doc);
        }

        private static Dictionary<string, int> CreateDictionary(TextDocument doc)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            string text = doc.Text;

            MatchCollection collection = sm_regex.Matches(text);

            foreach (Match match in collection)
            {
                string val = match.Value.ToLower();

                //dict[val] = (dict.ContainsKey(val)) ? dict[val] + 1 : 1;

                if (dict.ContainsKey(val))
                    dict[val]++;
                else
                    dict.Add(val, 1);
            }

            return dict;
        }

        public IEnumerable<string> Words
        {
            get { return m_wordIndexCount.Keys; }
        }

        public int GetOccurrances(string word)
        {
            if( word.Contains(word) )
                return m_wordIndexCount[word];

            return 0;
        }
    }
}
