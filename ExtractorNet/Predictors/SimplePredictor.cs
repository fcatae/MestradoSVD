using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentSimplePredictor : ITask
    {
        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new SimplePredictor(), (IDataSource)param);

            return result;
        }
    }

    class SimplePredictor : IPredictor
    {
        private string m_mostCommonCategory = null;
        private int m_highestDocumentCount = -1;

        public void Train(DocumentGroup[] categorygroup)
        {
            // Identify the most used category
            foreach (DocumentGroup group in categorygroup)
            {
                string categoryName = group.Name;
                int documentCount = group.GetDocuments().Length;

                UpdateMostCommonCategory(categoryName, documentCount);
            }
        }

        public void PreProcess(DocumentGroup group)
        {
        }

        public string Predict(Document doc)
        {
            return m_mostCommonCategory;
        }

        void UpdateMostCommonCategory(string categoryName, int documentCount)
        {
            if (documentCount > m_highestDocumentCount)
            {
                m_mostCommonCategory = categoryName;
                m_highestDocumentCount = documentCount;
            }
        }
    }

}
