using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentLsaCategoryPredictor : ITask
    {
        private int m_dimensions;

        public ExperimentLsaCategoryPredictor(int dimensions)
        {
            m_dimensions = dimensions;
        }

        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new LsaCategoryPredictor(m_dimensions), (IDataSource)param);

            return result;
        }
    }

    class ExperimentLsaProjCategoryPredictor : ITask
    {
        private int m_dimensions;

        public ExperimentLsaProjCategoryPredictor(int dimensions)
        {
            m_dimensions = dimensions;
        }

        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new LsaProjCategoryPredictor(m_dimensions), (IDataSource)param);

            return result;
        }
    }

    class LsaCategoryPredictor : IPredictor
    {
        IPredictor m_predictor;
        private WordDictionary m_dict;
        List<string> m_categoryNames;
        DocumentGroup[] m_trainingCategoryGroup;

        public LsaCategoryPredictor(int dimensions)
        {
            IPredictor predictor;

            m_dict = new FilteredWordDictionary(10); 

            predictor = new IdfVectorPredictor(new VectorPredictor(m_dict));

            predictor = new LatentSemanticAnalysisPredictor(dimensions, predictor);

            m_predictor = predictor;
           
        }

        static string GetCategoryToken(string name)
        {
            return ("CATEGORY" + name + "CATEGORY").Replace("-", "").ToLower();
        }

        void IncludeCategory(DocumentGroup group, WordDictionary dict)
        {
            string token = GetCategoryToken(group.Name);

            string repeat_category = "".PadRight(100, '1').Replace("1", token + " ");

            Bow bow = new Bow(new TextDocument(repeat_category));

            dict.AddBow(bow);

            m_categoryNames.Add(group.Name);
        }

        void AdjustVectorDocuments(DocumentGroup documentGroup)
        {
            Document[] documents = documentGroup.GetDocuments();
            int totalCategories = m_categoryNames.Count;

            Debug.Assert(documents[0].Vector != null);

            string token = GetCategoryToken(documentGroup.Name);
            int category_id = m_dict.TryGetIdentifier(token);

            Debug.Assert(category_id != WordDictionary.InvalidIdentifier);
            Debug.Assert((category_id >= 0) && (category_id < totalCategories));


            // Validate the dictionary
            bool bValidDictionary = ValidateDictionary(m_dict, m_categoryNames);

            foreach (Document doc in documents)
            {
                Vector vector = doc.Vector;                

                Debug.Assert(vector != null);

                for (int i = 0; i < totalCategories; i++)
                {
                    //vector.Values[i] = 0.001*Math.Log(1000.0 / documents.Length); //Vector.GetModule(vector);
                    if (i == category_id)
                        //vector.Values[i] = Vector.GetModule(vector);
                        vector.Values[i] = 0.001 * Math.Log(m_categoryNames.Count); //Vector.GetModule(vector);
                    //else
                    //    vector.Values[i] = -0.1 * Math.Log(m_categoryNames.Count/ (m_categoryNames.Count-1)); //-Vector.GetModule(vector);
                }
            }
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            m_categoryNames = new List<string>();

            // Include the categories in the dictionary
            foreach (DocumentGroup group in trainingCategoryGroup)
                IncludeCategory(group, m_dict);

            // Transform the text into vectors
            m_predictor.Train(trainingCategoryGroup);

            m_trainingCategoryGroup = trainingCategoryGroup;
        }

        static bool ValidateDictionary(WordDictionary dict, List<string> categoryNames)
        {
            int totalCategories = categoryNames.Count;

            foreach (string category in categoryNames)
            {
                string token = GetCategoryToken(category);
                int wordid = dict.TryGetIdentifier(token);

                Debug.Assert(wordid != WordDictionary.InvalidIdentifier);
                Debug.Assert((wordid >=0) &&(wordid<totalCategories));

                if (wordid == WordDictionary.InvalidIdentifier)
                    throw new InvalidOperationException();
            }

            return true;
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_predictor.PreProcess(documentGroup);
            
            // Adjust the vectors
            foreach (DocumentGroup group in m_trainingCategoryGroup)
                AdjustVectorDocuments(group);
        }

        public string Predict(Document sampleDocument)
        {
            return m_predictor.Predict(sampleDocument);
        }
    }

    class LsaProjCategoryPredictor : IPredictor
    {
        LsaCategoryPredictor m_predictor;
        List<string> m_categoryNames;
        DocumentGroup[] m_trainingCategoryGroup;
        
        public LsaProjCategoryPredictor(int dimensions) : this(new LsaCategoryPredictor(dimensions))
        {
        }

        public LsaProjCategoryPredictor(LsaCategoryPredictor preditor)
        {
            m_predictor = preditor;
            m_categoryNames = new List<string>();
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            //// may generate false positive - we are not comparing the content
            //Debug.Assert(trainingCategoryGroup == m_trainingCategoryGroup);

            foreach (DocumentGroup group in trainingCategoryGroup)
                m_categoryNames.Add(group.Name);

            m_predictor.Train(trainingCategoryGroup);

            m_trainingCategoryGroup = trainingCategoryGroup;
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_predictor.PreProcess(documentGroup);
        }

        public string Predict(Document sampleDocument)
        {
            string vectoredPrediction = null;
            double vectorMinimum = Double.MinValue;
            int totalCategories = m_categoryNames.Count;

            double[] categories = sampleDocument.Vector.Values;

            for (int i = 0; i < totalCategories; i++)
            {
                if (categories[i] > vectorMinimum)
                {
                    vectorMinimum = categories[i];
                    vectoredPrediction = m_categoryNames[i];
                }
            }

            return vectoredPrediction;
        }
    }

}
