using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentIdfVectorPredictor : ITask
    {
        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new IdfVectorPredictor(), (IDataSource)param);

            return result;
        }
    }

    class ExperimentIdfVectorPredictor10 : ITask
    {
        WordWeight m_weight;

        public ExperimentIdfVectorPredictor10(WordWeight weight)
        {
            m_weight = weight;
        }

        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(
                new IdfVectorPredictor(StandardPredictors.VectorMin10(), m_weight), (IDataSource)param);

            return result;
        }
    }

    class ExperimentIDFNormalizedVectorPredictor10 : ITask
    {
        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(
                new NormalizedVectorPredictor(new IdfVectorPredictor(StandardPredictors.VectorMin10())),
                (IDataSource)param);

            return result;
        }
    }

    enum WordWeight { TFIDF, VTFIDF }; //, DVTFIDF, TF_SQDF, VTF_SQDF, DVTF_SQDF };

    class IdfVectorPredictor : IPredictor
    {
        VectorPredictor m_vectorPredictor;
        double[] m_wordDocumentFrequency;
        double[] m_wordFrequency;
        int m_totalDocuments = 0;
        double m_totalWordsInAllDocuments = 0;
        WordWeight m_wordWeight;

        public IdfVectorPredictor() : this(new VectorPredictor())
        {
        }

        public IdfVectorPredictor(VectorPredictor vectorPredictor) : this(vectorPredictor, WordWeight.TFIDF)
        {            
        }

        public IdfVectorPredictor(VectorPredictor vectorPredictor, WordWeight weight)
        {
            m_vectorPredictor = vectorPredictor;
            m_wordWeight = weight;
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            m_vectorPredictor.Train(trainingCategoryGroup);

            CreateDocumentFrequency();

            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                Document[] docs = group.GetDocuments();

                UpdateWordDocumentFrequency(docs);
            }

             foreach (DocumentGroup group in trainingCategoryGroup)
            {
                foreach (Document doc in group.GetDocuments())
                {
                    CalculateTFIDF(doc.Vector);
                }
            }
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_vectorPredictor.PreProcess(documentGroup);

            foreach (Document doc in documentGroup.GetDocuments())
            {
                CalculateTFIDF(doc.Vector);
            }
        }

        public string Predict(Document sampleDocument)
        {
            string result = m_vectorPredictor.Predict(sampleDocument);

            return result;
        }

        void CreateDocumentFrequency()
        {
            int dimensions = m_vectorPredictor.Dimensions;

            Debug.Assert(dimensions > 0);

            m_wordDocumentFrequency = new double[dimensions];
            m_wordFrequency = new double[dimensions];
        }

        void UpdateWordDocumentFrequency(Document[] docs)
        {
            foreach (Document doc in docs)
            {
                Vector vect = doc.Vector;
                
                Debug.Assert(vect != null);

                CountWordPerDocumentFrequency(vect);
            }
        }

        void CountWordPerDocumentFrequency(Vector vect)
        {
            int dimensions = m_vectorPredictor.Dimensions;

            double[] words = vect.Values;

            Debug.Assert(words.Length == dimensions);

            for(int i=0; i<dimensions; i++)
            {
                if (words[i] > 0)
                {
                    m_wordDocumentFrequency[i]++;
                    m_wordFrequency[i] += words[i];
                    m_totalWordsInAllDocuments += words[i];
                }
            }

            m_totalDocuments++;
        }

        void CalculateTFIDF(Vector vector)
        {
            int dimensions = m_vectorPredictor.Dimensions;
            double[] vect = vector.Values;

            double term_occurrences;
            double words_document = CalculateSumDimension(vect);
            double total_documents = m_totalDocuments;
            double documents_with_word;

            for (int wordid = 0; wordid < dimensions; wordid++)
            {
                term_occurrences = vect[wordid];
                documents_with_word = m_wordDocumentFrequency[wordid];

                if (documents_with_word == 0)
                {
                    // Assume tf-idf=0
                    vect[wordid] = 0;
                    continue;
                }

                double words_all_documents = m_wordFrequency[wordid];
                double term_occurrences_plus1 = term_occurrences + (words_all_documents / m_totalWordsInAllDocuments);
                double words_document_plus1 = words_document + 1.0;

                //// calculate tf-idf
                double tf = (term_occurrences / words_document);

                double vtf = (term_occurrences / words_document) - (words_all_documents / m_totalWordsInAllDocuments);

                //double dvtf = (term_occurrences - (words_all_documents / m_totalWordsInAllDocuments));

                double idf = Math.Log(total_documents / documents_with_word);

                // TODO: Implement a post entropy-idf
                //double sidf = Math.Sqrt(documents_with_word / total_documents);

                // TFIDF, VTFIDF, DVTFIDF, TF_SQDF, VTF_SQDF, DVTF_SQDF, 
                // store the value

                switch (m_wordWeight)
                {
                    case WordWeight.TFIDF:          vect[wordid] = tf * idf;    break;
                    case WordWeight.VTFIDF:         vect[wordid] = vtf * idf;   break;
                    //case WordWeight.DVTFIDF:        vect[wordid] = dvtf * idf;  break;
                    //case WordWeight.TF_SQDF:        vect[wordid] = tf * sidf;   break;
                    //case WordWeight.VTF_SQDF:       vect[wordid] = vtf * sidf;  break;
                    //case WordWeight.DVTF_SQDF:      vect[wordid] = dvtf * sidf; break;
                    default:                        vect[wordid] = Double.NaN; Debug.Assert(false); break;
                }

                //vect[wordid] = dvtf * sidf;
                Debug.Assert(!Double.IsNaN(vect[wordid]));
                Debug.Assert(!Double.IsInfinity(vect[wordid]));
            }
        }

        double CalculateSumDimension(double[] vect)
        {
            int dimensions = m_vectorPredictor.Dimensions;
            double sum = 0;

            for (int i = 0; i < dimensions; i++)
                sum += vect[i];

            return sum;
        }
    }

    class IdfVectorPredictor2 : IPredictor
    {
        VectorPredictor m_vectorPredictor;
        double[] m_wordDocumentFrequency;
        int m_totalDocuments = 0;

        public IdfVectorPredictor2()
        {
            m_vectorPredictor = new VectorPredictor();
        }

        public IdfVectorPredictor2(VectorPredictor vectorPredictor)
        {
            m_vectorPredictor = vectorPredictor;
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            m_vectorPredictor.Train(trainingCategoryGroup);

            CreateDocumentFrequency();

            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                Document[] docs = group.GetDocuments();

                UpdateWordDocumentFrequency(docs);
            }

            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                foreach (Document doc in group.GetDocuments())
                {
                    CalculateTFIDF(doc.Vector);
                }
            }
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_vectorPredictor.PreProcess(documentGroup);

            foreach (Document doc in documentGroup.GetDocuments())
            {
                CalculateTFIDF(doc.Vector);
            }
        }

        public string Predict(Document sampleDocument)
        {
            string result = m_vectorPredictor.Predict(sampleDocument);

            return result;
        }

        void CreateDocumentFrequency()
        {
            int dimensions = m_vectorPredictor.Dimensions;

            Debug.Assert(dimensions > 0);

            m_wordDocumentFrequency = new double[dimensions];
        }

        void UpdateWordDocumentFrequency(Document[] docs)
        {
            foreach (Document doc in docs)
            {
                Vector vect = doc.Vector;

                Debug.Assert(vect != null);

                CountWordPerDocumentFrequency(vect);
            }
        }

        void CountWordPerDocumentFrequency(Vector vect)
        {
            int dimensions = m_vectorPredictor.Dimensions;

            double[] words = vect.Values;

            Debug.Assert(words.Length == dimensions);

            for (int i = 0; i < dimensions; i++)
            {
                if (words[i] > 0)
                    m_wordDocumentFrequency[i]++;
            }

            m_totalDocuments++;
        }

        void CalculateTFIDF(Vector vector)
        {
            int dimensions = m_vectorPredictor.Dimensions;
            double[] vect = vector.Values;

            double term_occurrences;
            double words_document = CalculateSumDimension(vect);
            double total_documents = m_totalDocuments;
            double documents_with_word;

            for (int wordid = 0; wordid < dimensions; wordid++)
            {
                term_occurrences = vect[wordid];
                documents_with_word = m_wordDocumentFrequency[wordid];

                if (documents_with_word == 0)
                {
                    // Assume tf-idf=0
                    vect[wordid] = 0;
                    continue;
                }

                // calculate tf-idf
                double tf = term_occurrences / words_document;

                double idf = Math.Log(total_documents / documents_with_word);

                // store the value
                vect[wordid] = tf * idf;
            }
        }

        double CalculateSumDimension(double[] vect)
        {
            int dimensions = m_vectorPredictor.Dimensions;
            double sum = 0;

            for (int i = 0; i < dimensions; i++)
                sum += vect[i];

            return sum;
        }
    }

}


        //// Backup the original TFIDF function
        //void CalculateTFIDF_Original(Vector vector)
        //{
        //    int dimensions = m_vectorPredictor.Dimensions;
        //    double[] vect = vector.Values;

        //    double term_occurrences;
        //    double words_document = CalculateSumDimension(vect);
        //    double total_documents = m_totalDocuments;
        //    double documents_with_word;

        //    for (int wordid = 0; wordid < dimensions; wordid++)
        //    {
        //        term_occurrences = vect[wordid];
        //        documents_with_word = m_wordDocumentFrequency[wordid];

        //        if (documents_with_word == 0)
        //        {
        //            // Assume tf-idf=0
        //            vect[wordid] = 0;
        //            continue;
        //        }

        //        // calculate tf-idf
        //        double tf = term_occurrences/words_document;

        //        double idf = Math.Log( total_documents/documents_with_word );

        //        // store the value
        //        vect[wordid] = tf * idf;
        //    }
        //}

