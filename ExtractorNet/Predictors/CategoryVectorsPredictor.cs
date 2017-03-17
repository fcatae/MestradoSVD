using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentCategoryVectorsPredictor : ITask
    {
        double m_dimensionPercent;
        int m_dimensionMinimum;

        public ExperimentCategoryVectorsPredictor(double percent, int minimum)
        {
            Debug.Assert((percent != 0.0) || (minimum != 0));

            m_dimensionPercent = percent;
            m_dimensionMinimum = minimum;
        }

        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new CategoryVectorsPredictor(m_dimensionPercent, m_dimensionMinimum), (IDataSource)param);

            return result;
        }
    }

    class CategoryVectorsPredictor : IPredictor
    {
        double m_dimensionPercent;
        int m_dimensionMinimum;

        IPredictor m_predictor;
        DocumentGroup[] m_categoryGroup;
        
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

        public CategoryVectorsPredictor() : this(0.6, 5)
        {
        }

        public CategoryVectorsPredictor(double percent, int minimum)
        {
            Debug.Assert((percent!=0.0) || (minimum!=0));

            m_dimensionPercent = percent;
            m_dimensionMinimum = minimum;

            m_predictor = new IdfVectorPredictor(new VectorPredictor(new FilteredWordDictionary(10)));
        }

        public CategoryVectorsPredictor(IPredictor predictor)
        {
            Debug.Assert( predictor != null );
            
            m_predictor = predictor;
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            IPredictor vector = m_predictor;

            vector.Train(trainingCategoryGroup);

            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                MatrixSVD decomp = new MatrixSVD(group);

                decomp.Init();

                int dimension = (int)(decomp.MaxDimension * m_dimensionPercent + .5) + m_dimensionMinimum;
                Debug.Assert(dimension > 0);


                // HACK: looking for adjustable improvements
                //if (group.Name == "earn")
                //    dimension = 50;
                //if (group.Name == "acq")
                //    dimension = 50;

                decomp.GetBaseVectors(dimension);
            }

            m_categoryGroup = trainingCategoryGroup;
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_predictor.PreProcess(documentGroup);
        }

        public string Predict(Document sampleDocument)
        {
            return Predict_Projection(sampleDocument);
        }

        public string Predict_Projection(Document sampleDocument)
        {
            DocumentGroup[] categorygroup = m_categoryGroup;
            Vector vectSample = sampleDocument.Vector;
            double vectModuleSample = Vector.GetModule(vectSample);

            Debug.Assert(vectSample != null);

            //List<VectorPredictorResult> resultset = new List<VectorPredictorResult>();
            List<VectorPredictorResult> resultsetSum = new List<VectorPredictorResult>();

            foreach (DocumentGroup group in categorygroup)
            {
                string name = group.Name;
                Document[] docs = group.GetDocuments();

                double total_score = 0;
                
                foreach (Document doc in docs)
                {
                    Vector vect = doc.Vector;
                    Debug.Assert(vect != null);

                    Debug.Assert(Vector.IsUnitOrNull(vect));

                    double score = Vector.Dot(vectSample, vect) / vectModuleSample;
                    
                    Debug.Assert(!Double.IsNaN(score));
                    Debug.Assert((score >= -1.00001) && (score <= 1.00001));

                    //resultset.Add(new VectorPredictorResult(name, score));

                    total_score += score * score;

                    Debug.Assert(total_score <= 1.00001);
                }

                resultsetSum.Add(new VectorPredictorResult(name, total_score));
            }

            //VectorPredictorResult maxResult = resultset.Max();
            VectorPredictorResult maxResultSum = resultsetSum.Max();

            return maxResultSum.Name;
        }

    }

}
