using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentTFIDF : ITask
    {
        // Implement this class
        public object Execute(object param)
        {
            FormDebugOutputTraceListener.Start();

            foreach (WordWeight w in Enum.GetValues( typeof(WordWeight)) )
            {
                Teste(w);
            }

            FormDebugOutputTraceListener.End();

            return "Finished";
        }

        void Teste(WordWeight weight)
        {
            IDataSource data_source = new Data.DataSample();
            IPredictor predictor = new IdfVectorPredictor(new VectorPredictor(new FilteredWordDictionary(0)), weight);

            Debug.WriteLine(weight.ToString());

            List<DataDocument> documents = new List<DataDocument>();

            documents.AddRange(data_source.GetTrainingSet());
            documents.AddRange(data_source.GetDocuments());

            DocumentGroup group = new DocumentGroup("all-docs");
            foreach (DataDocument doc in documents)
                group.Add(doc);

            predictor.Train(new DocumentGroup[] { group });

            Document[] matrix = group.GetDocuments();
            PrintMatrix(matrix);

            Debug.WriteLine("");
        }

        static void PrintMatrix(Document[] documents)
        {
            int cWords = documents[0].Vector.Values.Length;
            int cDocs = documents.Length;

            Debug.WriteLine("Matrix Output");
            for (int w = 0; w < cWords; w++)
            {
                for (int d = 0; d < cDocs; d++)
                {
                    double[] vector = documents[d].Vector.Values;
                    string value = vector[w].ToString("F5").PadLeft(10);
                    Debug.Write(value);
                }

                Debug.WriteLine("");
            }

            Debug.WriteLine("");
        }

        class DocumentStorePredictor : IPredictor
        {
            IPredictor m_predictor;

            public DocumentStorePredictor(IPredictor predictor)
            {
                m_predictor = predictor;
            }

            public void Train(DocumentGroup[] trainingCategoryGroup)
            {
                m_predictor.Train(trainingCategoryGroup);
            }

            public void PreProcess(DocumentGroup documentGroup)
            {
                m_predictor.PreProcess(documentGroup);
            }

            public string Predict(Document sampleDocument)
            {
                return "";
            }
        }
    }

    class ExperimentDVTFIDF : ITask
    {
        // Implement this class
        public object Execute(object param)
        {
            FormDebugOutputTraceListener.Start();

            foreach (WordWeight w in Enum.GetValues(typeof(WordWeight)))
            {
                Teste(w);
            }

            FormDebugOutputTraceListener.End();

            return "Finished";
        }

        void Teste(WordWeight weight)
        {
            IDataSource data_source = new Data.DataRepeat();
            IPredictor predictor = new IdfVectorPredictor(new VectorPredictor(new FilteredWordDictionary(0)), weight);

            Debug.WriteLine(weight.ToString());

            List<DataDocument> documents = new List<DataDocument>();

            documents.AddRange(data_source.GetTrainingSet());
            documents.AddRange(data_source.GetDocuments());

            DocumentGroup group = new DocumentGroup("all-docs");
            foreach (DataDocument doc in documents)
                group.Add(doc);

            predictor.Train(new DocumentGroup[] { group });

            Document[] matrix = group.GetDocuments();
            PrintMatrix(matrix);

            Debug.WriteLine("");
        }

        static void PrintMatrix(Document[] documents)
        {
            int cWords = documents[0].Vector.Values.Length;
            int cDocs = documents.Length;

            Debug.WriteLine("Matrix Output");
            for (int w = 0; w < cWords; w++)
            {
                for (int d = 0; d < cDocs; d++)
                {
                    double[] vector = documents[d].Vector.Values;
                    string value = vector[w].ToString("F5").PadLeft(10);
                    Debug.Write(value);
                }

                Debug.WriteLine("");
            }

            Debug.WriteLine("");
        }

        class DocumentStorePredictor : IPredictor
        {
            IPredictor m_predictor;

            public DocumentStorePredictor(IPredictor predictor)
            {
                m_predictor = predictor;
            }

            public void Train(DocumentGroup[] trainingCategoryGroup)
            {
                m_predictor.Train(trainingCategoryGroup);
            }

            public void PreProcess(DocumentGroup documentGroup)
            {
                m_predictor.PreProcess(documentGroup);
            }

            public string Predict(Document sampleDocument)
            {
                return "";
            }
        }
    }
    

}
