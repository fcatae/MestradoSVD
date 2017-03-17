using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentDimensionPredictor : ITask
    {
        private double m_tolerance;

        public ExperimentDimensionPredictor(double tolerance)
        {
            m_tolerance = tolerance;
        }

        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new SvdDimensionPredictor(m_tolerance), (IDataSource)param);

            return result;
        }
    }

    class SvdDimensionPredictor : IPredictor
    {
        IPredictor m_predictor;

        private double m_tolerance;

        public SvdDimensionPredictor(double tolerance)
        {
            Debug.Assert(tolerance > 0.0);

            m_tolerance = tolerance;
            m_predictor = new IdfVectorPredictor(new VectorPredictor(new FilteredWordDictionary(10)));
        }

        public SvdDimensionPredictor(double tolerance, IPredictor predictor)
        {
            Debug.Assert(tolerance > 0.0);
            Debug.Assert( predictor != null );

            m_tolerance = tolerance;
            m_predictor = predictor;
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            IPredictor vector = m_predictor;
            

            vector.Train(trainingCategoryGroup);

            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                MatrixSVD decomp = new MatrixSVD(group);
                double tolerance = this.m_tolerance;

                // comment: this hack doesnt work - no visible improvements for precision x recall
                //
                //if(( group.Name == "crude" )||( group.Name == "interest" )||( group.Name == "carcass")||( group.Name == "ship"))
                //    tolerance = .80;
                //if(( group.Name == "earn" )||( group.Name == "gran" )||( group.Name == "gold")||( group.Name == "cpi"))
                //    tolerance = .01;
   
                decomp.Init();

                int dim = 1;
                int dim_step = decomp.MaxDimension / 2;

                Debug.WriteLine("Group {0}: dim {1}", group.Name, decomp.MaxDimension);

                double err = 100000;
                double err_prev = 100000;

                for (int pass = 0; pass < 10; pass++)
                {
                    if (err > tolerance)
                        dim += dim_step;
                    else
                        dim -= dim_step;

                    decomp.SetDimension(dim);
                    err = decomp.GetApproximationError();

                    Debug.WriteLine("      err={0:F5}-dim:{1}", err, dim);

                    dim_step /= 2;

                    if (dim_step == 0)
                        break;

                    if (Math.Abs(err - err_prev) < .01)
                        break;

                    err_prev = err;
                }

                double err_final = decomp.GetApproximationError();

                Debug.WriteLine("    FINAL Dim={0}/{1} ERR={2:F5}", dim, decomp.MaxDimension, err_final);

                Debug.Assert((Math.Abs(err_final) < 1e-10) || (decomp.MaxDimension > dim));
            }
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            m_predictor.PreProcess(documentGroup);
        }

        public string Predict(Document sampleDocument)
        {
            return m_predictor.Predict(sampleDocument);
        }
    }

}
