using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentWordFilterPredictor : ITask
    {
        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new WordFilterPredictor(100), (IDataSource)param);

            return result;
        }
    }

    class FilteredWordDictionary : WordDictionary
    {
        private WordDictionaryStat m_dictionaryStat = new WordDictionaryStat();
        private int m_minimum;

        public FilteredWordDictionary(int minimum)
        {
            m_minimum = minimum;
        }

        public override void AddBow(Bow bow)
        {
            m_dictionaryStat.AddBow(bow);
        }

        public override void Complete()
        {
            int minimum = m_minimum;
            bool isValidDictionary = false;

            foreach (var stat in m_dictionaryStat.GetStatCount())
            {
                if ((stat.Count >= minimum)) //&& (stat.Word.Length >=3 ))
                {
                    isValidDictionary = true;
                    UpdateWordCount(stat.Word, stat.Count);
                }
            }

            Debug.Assert(isValidDictionary);

            base.Complete();

            m_dictionaryStat = null;
        }
    }

    class WordFilterPredictor : VectorPredictor
    {
        public WordFilterPredictor(int minimum) : base(new FilteredWordDictionary(minimum))
        {
        }

    }

}
