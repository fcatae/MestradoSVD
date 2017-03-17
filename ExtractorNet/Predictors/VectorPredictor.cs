using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentVectorPredictor : ITask
    {
        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new VectorPredictor(), (IDataSource)param);

            return result;
        }
    }

    class ExperimentNormalizedVectorPredictor : ITask
    {
        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new NormalizedVectorPredictor(), (IDataSource)param);

            return result;
        }
    }

    class NormalizedVectorPredictor : IPredictor
    {
        IPredictor m_vectorPredictor;

        public NormalizedVectorPredictor()
        {
            m_vectorPredictor = new VectorPredictor();
        }

        public NormalizedVectorPredictor(IPredictor vectorPredictor)
        {
            m_vectorPredictor = vectorPredictor;
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            m_vectorPredictor.Train(trainingCategoryGroup);

            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                Document[] docs = group.GetDocuments();

                NormalizeVectors(docs);
            }
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_vectorPredictor.PreProcess(documentGroup);

            Document[] docs = documentGroup.GetDocuments();

            NormalizeVectors(docs);
        }

        public string Predict(Document sampleDocument)
        {
            string result = m_vectorPredictor.Predict(sampleDocument);

            return result;
        }

        void NormalizeVectors(Document[] docs)
        {
            foreach (Document doc in docs)
            {
                Vector vect = doc.Vector;

                vect.Normalize();
            }
        }
    }

    class VectorPredictor : IPredictor
    {
        class VectorPredictorResult : IComparable<VectorPredictorResult>
        {
            public VectorPredictorResult(string name, double score)
            {
                this.Name = name;
                this.Score = score;
            }

            public string Name;
            public double Score;

            public int CompareTo(VectorPredictorResult other)
            {
                if (other == null) return 1;

                return Score.CompareTo(other.Score);
            }
        }

        WordDictionary m_dictionary;
        DocumentGroup[] m_categoryGroup;
        bool m_skipVectorization;

        public VectorPredictor() : this(new WordDictionary())
        {
        }
        
        public VectorPredictor(WordDictionary dictionary) : this(dictionary,false)
        {
        }

        public VectorPredictor(WordDictionary dictionary, bool bSkipVectorization)
        {
            Debug.Assert((dictionary != null) || bSkipVectorization );

            m_dictionary = dictionary;
            m_skipVectorization = bSkipVectorization;
        }

        public int Dimensions
        {
            get { return m_dictionary.TotalWords; }
        }

        public WordDictionary WordDictionary
        {
            get { return m_dictionary; }
        }

        public void Train(DocumentGroup[] categorygroup)
        {
            m_categoryGroup = categorygroup;

            foreach (DocumentGroup group in categorygroup)
            {
                Document[] docs = group.GetDocuments();

                PrepareTextIntoBows(docs);

                UpdateDictionary(docs);
            }

            SetDictionaryComplete();
            
            foreach (DocumentGroup group in categorygroup)
            {
                Document[] docs = group.GetDocuments();

                TransformBowsIntoVectors(docs);
            }
        }

        public void PreProcess(DocumentGroup group)
        {
            Document[] docs = group.GetDocuments();

            PrepareTextIntoBows(docs);

            TransformBowsIntoVectors(docs);
        }

        public string Predict(Document sampleDocument)
        {
            return Predict_Nearest(sampleDocument);
            //return Predict_Centroid(sampleDocument);            
        }

        public string Predict_Centroid(Document sampleDocument)
        {
            DocumentGroup[] categorygroup = m_categoryGroup;
            Vector vectSample = sampleDocument.Vector;

            Debug.Assert(vectSample != null);

            List<VectorPredictorResult> resultset = new List<VectorPredictorResult>();

            foreach (DocumentGroup group in categorygroup)
            {
                string name = group.Name;
                Document[] docs = group.GetDocuments();

                double total_score = 0;
                double total_docs = 0;

                foreach (Document doc in docs)
                {
                    Vector vect = doc.Vector;
                    Debug.Assert(vect != null);

                    double score = Vector.Dot(vectSample, vect);
                    //double score = Vector.Cosine(vectSample, vect);

                    total_score += ( !Double.IsNaN(score) ) ? score : 0.0;
                    total_docs += 1.0;
                }

                resultset.Add(new VectorPredictorResult(name, total_score / total_docs));
            }

            VectorPredictorResult maxResult = resultset.Max();

            return maxResult.Name;
        }

        public string Predict_Nearest(Document sampleDocument)
        {
            DocumentGroup[] categorygroup = m_categoryGroup;
            Vector vectSample = sampleDocument.Vector;

            Debug.Assert(vectSample != null);

            List<VectorPredictorResult> resultset = new List<VectorPredictorResult>();

            foreach (DocumentGroup group in categorygroup)
            {
                string name = group.Name;
                Document[] docs = group.GetDocuments();

                foreach (Document doc in docs)
                {
                    Vector vect = doc.Vector;
                    Debug.Assert(vect != null);

                    double score = Vector.Dot(vectSample, vect);

                    resultset.Add(new VectorPredictorResult(name, score));
                }
            }

            VectorPredictorResult maxResult = resultset.Max();

            return maxResult.Name;
        }

        void PrepareTextIntoBows(Document[] docs)
        {
            if (m_skipVectorization)
            {
                foreach (Document doc in docs)
                    Debug.Assert(doc.Vector != null);
 
                return;
            }
            
            foreach (Document doc in docs)
            {
                Bow bow = new Bow(doc.TextDocument);

                doc.Set(bow);
            }
        }

        void UpdateDictionary(Document[] docs)
        {
            if (m_skipVectorization)
            {
                foreach (Document doc in docs)
                    Debug.Assert(doc.Vector != null);

                return;
            }

            WordDictionary dict = m_dictionary;

            foreach (Document doc in docs)
            {
                dict.AddBow(doc.Bow);
            }
        }

        void SetDictionaryComplete()
        {
            if (m_skipVectorization)
            {
                // there is no dictionary
                Debug.Assert((m_dictionary == null) ||
                            ((m_dictionary != null) && (m_dictionary.IsComplete)) );
                return;
            }

            m_dictionary.Complete();
        }

        void TransformBowsIntoVectors(Document[] docs)
        {
            if (m_skipVectorization)
            {
                foreach (Document doc in docs)
                    Debug.Assert(doc.Vector != null);

                return;
            }

            WordDictionary dict = m_dictionary;

            foreach (Document doc in docs)
            {
                Bow bow = doc.Bow;
                Vector vect = new Vector(bow, dict);

                doc.Set(vect);
            }
        }
    }

}
