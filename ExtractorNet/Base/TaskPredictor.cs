using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    public interface ITask
    {
        object Execute(object param);
    }

    interface IPredictor
    {
        void Train(DocumentGroup[] trainingCategoryGroup);
        void PreProcess(DocumentGroup documentGroup);
        string Predict(Document sampleDocument);
    }

    interface IPredictorStateful
    {
        void Train(DocumentGroup[] trainingCategoryGroup);
        void PreProcess(DocumentGroup documentGroup);
        string Predict(Document sampleDocument);
        
    }

    class StandardPredictors
    {
        public static VectorPredictor VectorMin10()
        {
            return new VectorPredictor(new FilteredWordDictionary(10));
        }
    }

    class TaskPredictor : ITask
    {
        class PredictorResults
        {
            public Document Document;
            public string ExpectedCategory;
            public string PredictedCategory;
        }

        class AnalyzerResults : Dictionary<string,int>
        {
            public int GetKey(string value)
            {
                if (this.ContainsKey(value))
                    return this[value];

                return 0;
            }
        }

        private IPredictor m_predictor;

        public TaskPredictor(IPredictor predictor)
        {
            m_predictor = predictor;
        }
        
        public object Execute(object param)
        {
            IDataSource datasource = (IDataSource)param;

            IPredictor predictor = m_predictor;
            DocumentGroup[] trainingSet = DocumentGroup.GroupCategories(datasource);
            DocumentGroup finalTestDocs = DocumentGroup.GetFinalData(datasource);

            return EvaluateScore(predictor, trainingSet, finalTestDocs);
        }

        public static double EvaluateScore(IPredictor predictor, IDataSource datasource)
        {
            DocumentGroup[] trainingSet = DocumentGroup.GroupCategories(datasource);
            DocumentGroup finalTestDocs = DocumentGroup.GetFinalData(datasource);

            return EvaluateScore(predictor, trainingSet, finalTestDocs);
        }

        public static double EvaluateScore(IPredictor predictor, DocumentGroup[] trainingSet, DocumentGroup finalTestDocs)
        {
            List<PredictorResults> results;
            double score;

            results = ExecutePredictor(predictor, trainingSet, finalTestDocs);

            score = CalculateResultScore(results);

            AnalyzePredictorResults(results, score);

            return score;
        }

        static void AnalyzePredictorResults(List<PredictorResults> results, double score)
        {
            Debug.WriteLine("AnalyzePredictorResults: Score={0:F2}%", score);

            AnalyzerResults categories;
            AnalyzerResults categories_total = new AnalyzerResults();
            AnalyzerResults categories_correct = new AnalyzerResults();
            AnalyzerResults categories_precision = new AnalyzerResults();
            AnalyzerResults categories_recall = new AnalyzerResults();

            foreach (PredictorResults result in results)
            {
                string expected = result.ExpectedCategory;
                string predicted = result.PredictedCategory;

                categories = categories_total;
                {
                    if (!categories.ContainsKey(expected))
                        categories.Add(expected, 0);
                    categories[expected]++;
                }

                if (expected != predicted)
                {
                    categories = categories_recall;
                    {
                        if (!categories.ContainsKey(expected))
                            categories.Add(expected, 0);
                        categories[expected]--;
                    }
                    categories = categories_precision;
                    {
                        if (!categories.ContainsKey(predicted))
                            categories.Add(predicted, 0);
                        categories[predicted]--;
                    }
                }
                else
                {
                    categories = categories_correct;
                    {
                        if (!categories.ContainsKey(expected))
                            categories.Add(expected, 0);
                        categories[expected]++;
                    }

                    categories = categories_recall;
                    {
                        if (!categories.ContainsKey(expected))
                            categories.Add(expected, 0);
                    }
                    categories = categories_precision;
                    {
                        if (!categories.ContainsKey(predicted))
                            categories.Add(predicted, 0);
                    }
                }

            }

            Debug.WriteLine("Results - ORDER BY Qty");
            foreach (var category_result in categories_total.OrderByDescending( s => s.Value ))
            {
                string category = category_result.Key;

                int total = categories_total.GetKey(category);
                int precision = categories_precision.GetKey(category);
                int recall = categories_recall.GetKey(category); //categories_total.GetKey(category) + 
                int correct = categories_correct.GetKey(category);
                Debug.WriteLine("Category: {0}. Total={1}/Precision={2}/Recall={3}/Correct={4}(recall:{5:F2},prec:{6:F2})/Attempts={7}", category, total, precision, recall, correct, 100.0 * correct / total, 100.0 * precision / total, correct - precision);
            }            
            Debug.WriteLine("");

            Debug.WriteLine("Results - ORDER BY Precision");
            foreach (var category_result in categories_precision.OrderBy(s => s.Value))
            {
                string category = category_result.Key;

                int total = categories_total.GetKey(category);
                int precision = categories_precision.GetKey(category);
                int recall = categories_recall.GetKey(category); //categories_total.GetKey(category) + 
                int correct = categories_correct.GetKey(category);
                Debug.WriteLine("Category: {0}. Total={1}/Precision={2}/Recall={3}/Correct={4}(recall:{5:F2},prec:{6:F2})/Attempts={7}", category, total, precision, recall, correct, 100.0 * correct / total, 100.0 * precision / total, correct - precision);
            }
            Debug.WriteLine("");

            Debug.WriteLine("Results - ORDER BY Recall");
            foreach (var category_result in categories_recall.OrderBy(s => s.Value))
            {
                string category = category_result.Key;

                int total = categories_total.GetKey(category);
                int precision = categories_precision.GetKey(category);
                int recall = categories_recall.GetKey(category); //categories_total.GetKey(category) + 
                int correct = categories_correct.GetKey(category);
                Debug.WriteLine("Category: {0}. Total={1}/Precision={2}/Recall={3}/Correct={4}(recall:{5:F2},prec:{6:F2})/Attempts={7}", category, total, precision, recall, correct, 100.0 * correct / total, 100.0 * precision / total, correct -precision);
            }
            Debug.WriteLine("");

        }

        static List<PredictorResults> ExecutePredictor(IPredictor predictor, DocumentGroup[] trainingSet, DocumentGroup finalTestDocs)
        {
            List<PredictorResults> finalExam = new List<PredictorResults>();

            predictor.Train(trainingSet);

            predictor.PreProcess(finalTestDocs);
            
            foreach (Document doc in finalTestDocs.GetDocuments())
            {
                string predictedCategory = predictor.Predict(doc);

                PredictorResults result = new PredictorResults()
                {
                    Document = doc,
                    ExpectedCategory = doc.Category,
                    PredictedCategory = predictedCategory
                };

                finalExam.Add(result);
            }

            return finalExam;
        }


        static double CalculateResultScore(IEnumerable<PredictorResults> resultset)
        {
            int correct = 0;
            int total = 0;

            foreach (PredictorResults result in resultset)
            {
                if (result.ExpectedCategory == result.PredictedCategory)
                    correct++;

                total++;
            }

            return ((double)100.0 * correct) / total;
        }
    }

}
