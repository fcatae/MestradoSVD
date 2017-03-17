using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    // Separar em apenas duas classes (preto e branco)
    class Experiment1 : ITask
    {
        private IDataSource m_dataSource;

        public Experiment1(IDataSource source)
        {
            m_dataSource = source;
        }

        public object Execute2(object param)
        {
            return null;
        }

        public object Execute(object param)
        {
            //DocumentGroup training = DocumentGroup.GetTrainingData(m_dataSource);
            //DocumentGroup[] groups = DocumentGroup.GroupCategories(m_dataSource);
            //DocumentGroup exam = DocumentGroup.GetFinalData(m_dataSource);

            //Document[] docs = training.GetDocuments();

            //WordDictionary dict = CreateDictionary(docs);

            LsaCategoryPredictor lsaCategoryPredictor = new LsaCategoryPredictor(100);

            //35
            //double result1 = TaskPredictor.EvaluateScore(
            //    new LsaCategoryPredictor(100), (IDataSource)param);

            //double result2 = TaskPredictor.EvaluateScore(
            //    new LsaProjCategoryPredictor(lsaCategoryPredictor), (IDataSource)param);

            double result1 = TaskPredictor.EvaluateScore(
                new CategorizedLSAPredictor(100), (IDataSource)param);

            double result2 = TaskPredictor.EvaluateScore(
                new CategorizedLSAPredictor(400), (IDataSource)param);            

            return result1;
        }

        //WordDictionary CreateDictionary(Document[] docs)
        //{
        //    WordDictionary dict = new FilteredWordDictionary(10);

        //    foreach (Document doc in docs)
        //    {
        //        Bow bow = new Bow(doc.TextDocument);
                                
        //        dict.AddBow(bow);

        //        doc.Set(bow);
        //    }

        //    dict.Complete();

        //    foreach (Document doc in docs)
        //    {
        //        Bow     bow  = doc.Bow;
        //        Vector  vect = new Vector(bow, dict);

        //        doc.Set(vect);
        //    }

        //    return dict;
        //}
        
    }


    class CategorizedLSAPredictor : IPredictor
    {
        DocumentGroup m_allDocGroup = new DocumentGroup("all");
        MatrixSVD m_svd;

        private IPredictor m_predictor;
        private WordDictionary m_dict;
        private int m_dimensions;
        private int m_totalCategories;
        List<string> m_categoryNames;

        public CategorizedLSAPredictor(int dimensions)
        {
            m_dimensions = dimensions;
            m_dict = new FilteredWordDictionary(10);
            m_predictor = new IdfVectorPredictor(new VectorPredictor(m_dict));
        }

        string GetCategoryToken(string name)
        {
            return ("CATEGORY" + name + "CATEGORY").Replace("-", "").ToLower();
        }

        void IncludeCategories(DocumentGroup[] groups, WordDictionary dict)
        {
            int count = 0;
            m_categoryNames = new List<string>();

            foreach (DocumentGroup group in groups)
            {
                string token = GetCategoryToken(group.Name);

                string repeat_category = "".PadRight(100, '1').Replace("1", token + " ");

                Bow bow = new Bow(new TextDocument(repeat_category));
                dict.AddBow(bow);

                m_categoryNames.Add(group.Name);
                count++;
            }

            m_totalCategories = count;
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            IncludeCategories(trainingCategoryGroup, m_dict);

            m_predictor.Train(trainingCategoryGroup);

            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                AdjustVectorDocuments(group);
                IncludeDocuments(group);
            }
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_predictor.PreProcess(documentGroup);

            IncludeDocuments(documentGroup);

            ProcessLSA(m_dimensions);
        }

        public string Predict(Document sampleDocument)
        {
            string previousPrediction = m_predictor.Predict(sampleDocument);
            double previousMinimum = Double.MinValue;
            string correctPrediction = sampleDocument.Category;
            double correctMinimum = Double.MinValue;
            string vectoredPrediction = null;
            double vectorMinimum = Double.MinValue;
            bool bReplay = false;

            double[] categories = new double[m_totalCategories];

            Vector vect = sampleDocument.Vector;

            Array.Copy(vect.Values, categories, categories.Length);

        replay:
            for (int i = 0; i < m_totalCategories; i++)
            {
                if (categories[i] > vectorMinimum)
                {
                    vectorMinimum = categories[i];
                    vectoredPrediction = m_categoryNames[i];
                }

                //if(bReplay) Debug.WriteLine("{0}\t:{1:F1}", m_categoryNames[i], Math.Log(categories[i]));

                if (m_categoryNames[i] == correctPrediction)
                    correctMinimum = (categories[i]);

                if (m_categoryNames[i] == previousPrediction)
                    previousMinimum = (categories[i]);
            }

            if (bReplay)
            {
                Debug.WriteLine("Minimum prediction ({0}): {1:F1}", vectoredPrediction, Math.Log(vectorMinimum));
                Debug.WriteLine("Previous prediction ({0}): {1:F1}", previousPrediction, Math.Log(previousMinimum));
                Debug.WriteLine("Correct prediction ({0}): {1:F1}", correctPrediction, Math.Log(correctMinimum));
                Debug.WriteLine("");
            }

            string guessPrediction = "";

            if (vectoredPrediction == correctPrediction)
                guessPrediction = vectoredPrediction;

            if (previousPrediction == correctPrediction)
                guessPrediction = previousPrediction;

            if (guessPrediction != vectoredPrediction)
            {
                // understand why
                if (!bReplay)
                {
                    bReplay = true;

                    goto replay;
                }
            }

            //Debug.WriteLine("");

            return previousPrediction;
        }

        void AdjustVectorDocuments(DocumentGroup documentGroup)
        {
            Document[] documents = documentGroup.GetDocuments();

            Debug.Assert(documents[0].Vector != null);

            string token = GetCategoryToken(documentGroup.Name);
            int category_id = m_dict.TryGetIdentifier(token);

            Debug.Assert(category_id != WordDictionary.InvalidIdentifier);

            foreach (Document doc in documents)
            {
                Vector vector = doc.Vector;

                Debug.Assert(vector != null);

                for (int i = 0; i < m_totalCategories; i++)
                {
                    if (i == category_id)
                        vector.Values[i] = Vector.GetModule(vector);
                    else
                        vector.Values[i] = 0;
                }
            }
        }

        void IncludeDocuments(DocumentGroup documentGroup)
        {
            Document[] documents = documentGroup.GetDocuments();

            Debug.Assert(documents[0].Vector != null);

            foreach (Document doc in documents)
            {
                m_allDocGroup.AddVirtual(doc);
            }
        }

        void ProcessLSA(int dimension)
        {
            MatrixSVD svd = m_svd;

            if (m_svd == null)
            {
                svd = new MatrixSVD(m_allDocGroup);
                svd.Init();
            }

            svd.SetDimension(dimension);

            double err = svd.GetApproximationError();

            m_svd = svd;
        }

    }

}


