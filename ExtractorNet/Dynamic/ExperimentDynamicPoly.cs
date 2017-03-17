using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentDynamicPolysize : ITask
    {
        public object Execute(object param)
        {
            FormDebugOutputTraceListener.Start();

            Func<double, double> f = PolynomialFunction.f50;
            DataValues[] samples = PolynomialFunction.CreateDataSet(f, 100);
            DataValues[] testset = PolynomialFunction.CreateDataSet(f, 100);

            DataGroup datag = new DataGroup(samples);

            datag.DefineDynamicRegion();

            // First attempt
            DataValues[] train = datag.GetLowTrainingData();

            PolynomialFunction polyFinal = new PolynomialFunction();
            polyFinal.Train(train);
            polyFinal.Predict(testset);

            double err = DataGroup.GetScore(testset);

            Debug.WriteLine("Ideal dimension={0} (final score={1:F5}, log={2:F1})", train.Length, err, Math.Log10(err));

            // Second attempt
            DataGroup datag_improv = datag.GetBestGroup();

            DataValues[] train2 = datag_improv.GetLowTrainingData();

            PolynomialFunction polyFinal2 = new PolynomialFunction();
            polyFinal2.Train(train2);
            polyFinal2.Predict(testset);

            double err2 = DataGroup.GetScore(testset);

            Debug.WriteLine("Improved DG dimension={0} (final score={1:F5}, log={2:F1})", train2.Length, err2, Math.Log10(err2));

            FormDebugOutputTraceListener.End();

            return err;
        }        
    }

    class DataGroup
    {
        class ExperimentDynResults
        {
            public double Score1;
            public double Score2;
        }

        private DataValues[] m_dataValues;
        private int m_trainingSize;
        private int m_feedbackSize;
        private int m_predictionSize;
        private int m_totalSize;
        private int m_startSize;
        private int m_endSize;
        private int m_predictionSplit1;
        private int m_predictionSplit2;
        private Random m_random = new Random(0);

        public int Start { get { return m_startSize; } }
        public int End { get { return m_endSize; } }

        public DataGroup(IEnumerable<DataValues> samples)
        {
            List<DataValues> data = new List<DataValues>();

            m_dataValues = samples.ToArray();

            int total = m_dataValues.Length;

            m_startSize = 0;
            m_endSize = total;
            m_totalSize = total;

            UpdateSizes();
        }

        void UpdateSizes()
        {
            Debug.Assert(m_endSize >= m_startSize);

            int newsize = m_endSize - m_startSize;

            if (newsize > 0)
            {
                int training = (newsize + 2) / 3;
                int training2 = (newsize ) / 3;

                m_trainingSize = m_startSize + training;
                m_feedbackSize = training2;
                m_predictionSize = m_totalSize - m_trainingSize -m_feedbackSize;

                m_predictionSplit1 = m_predictionSize / 2;
                m_predictionSplit2 = m_predictionSize - m_predictionSplit1;
            }

            Debug.Assert(m_trainingSize > 0);
            Debug.Assert(m_feedbackSize >= 0);
            Debug.Assert(m_predictionSplit1 > 0);
            Debug.Assert(m_predictionSplit2 > 0);

            Debug.Assert((m_trainingSize + m_feedbackSize + m_predictionSize) == m_totalSize);
        }

        public void Shuffle<T>(T[] array)
        {
            Random random = m_random;

            for (int i = array.Length; i > 1; i--)
            {
                // Pick random element to swap.
                int j = random.Next(i); // 0 <= j <= i-1
                // Swap.
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }
        }

        public DataGroup ResampledGroup()
        {
            //DataValues[] newdata = new DataValues[m_dataValues.Length];

            DataValues[] newdata = (DataValues[])m_dataValues.Clone();
            //m_dataValues.CopyTo(newdata, 0);

            Shuffle(newdata);

            return new DataGroup(newdata);
        }

        void AdjustDimension(int start)
        {
            m_startSize = start;
            m_endSize = start + 1;
            UpdateSizes();
        }

        public DataGroup ResampledGroup(int dimension)
        {
            DataGroup group = ResampledGroup();

            group.AdjustDimension(dimension);

            return group;
        }



        public DataValues[] GetMidTrainingData()
        {
            DataValues[] training = new DataValues[m_trainingSize+m_feedbackSize/2];

            Array.Copy(m_dataValues, training, training.Length);

            foreach (DataValues t in training)
            {
                t.Reset();
            }

            return training;
        }

        public DataValues[] GetMidPredictionData()
        {
            int training_size = m_trainingSize + m_feedbackSize / 2;

            DataValues[] prediction = new DataValues[m_totalSize - training_size];

            Array.Copy(m_dataValues, prediction, prediction.Length);

            foreach (DataValues t in prediction)
            {
                t.Reset();
            }

            return prediction;
        }

        public DataValues[] GetLowTrainingData()
        {
            DataValues[] training = new DataValues[m_trainingSize];

            Array.Copy(m_dataValues, training, training.Length);

            foreach (DataValues t in training)
            {
                t.Reset();
            }

            return training;
        }

        //GetFirstAttemptData
        public DataValues[] GetFeedbackData()
        {
            DataValues[] feedback = new DataValues[m_predictionSplit1+m_feedbackSize / 2];

            Array.Copy(m_dataValues, m_trainingSize, feedback, 0, feedback.Length);


            return feedback;
        }

        public DataValues[] GetHighTrainingData()
        {
            DataValues[] feedback = new DataValues[m_trainingSize + m_feedbackSize];

            Array.Copy(m_dataValues, feedback, feedback.Length);

            foreach (DataValues t in feedback)
            {
                t.Reset();
            }

            return feedback;
        }

        public DataValues[] GetPredictionData()
        {
            DataValues[] prediction = new DataValues[m_predictionSplit2 + m_feedbackSize/2];

            Array.Copy(m_dataValues, m_trainingSize + m_feedbackSize + m_predictionSplit1 - m_feedbackSize / 2, prediction, 0, prediction.Length);

            return prediction;
        }

        bool isSignificantSmaller(double score1, double score2)
        {
            return (score1 <= score2);
        }

        public void ProvideFeedback(double score1, double score2)
        {
            Debug.Assert(!Double.IsNaN(score1));
            Debug.Assert(!Double.IsNaN(score2));
            Debug.Assert(!Double.IsNaN(score1 - score2));

            if (isSignificantSmaller(score1, score2))
            {
                ProvideFeedbackDecreaseData();
            }
            else
            {
                ProvideFeedbackIncreaseData();
            }
        }

        public void ProvideFeedbackDecreaseData()
        {
            //int newsize = (2 * (m_endSize - m_startSize) + 2) / 3;

            //int endsize = m_startSize + newsize;
            int newsize = (m_endSize - m_startSize) / 3;
            int endsize = m_endSize - newsize;

            if (endsize != m_endSize)
                m_endSize = m_startSize + newsize;
            //else
            //    m_endSize--;

            //if (m_startSize == m_endSize)
            //    m_startSize = m_endSize - 1; // push down the boundary
            ////m_endSize = m_startSize + 1;

            UpdateSizes();

            Debug.Assert((m_trainingSize + m_feedbackSize + m_predictionSize) == m_totalSize);
        }

        public void ProvideFeedbackIncreaseData()
        {
            int newsize = (m_endSize - m_startSize) / 3;

            if (newsize > 0)
                m_startSize = m_startSize + newsize;
            //else
            //    m_startSize++;

            //if (m_startSize == m_endSize)
            //    m_endSize = m_startSize + 1; // push the boundary
            ////m_startSize = m_endSize - 1;

            UpdateSizes();

            Debug.Assert((m_trainingSize + m_feedbackSize + m_predictionSize) == m_totalSize);
        }


        public static double GetScore(IEnumerable<DataValues> dataValues)
        {
            double error = 0;
            int count = 0;

            foreach (DataValues data in dataValues)
            {
                error += data.Error;
                count++;
            }

            double result = Math.Sqrt(error) / count;

            Debug.Assert(!Double.IsNaN(result));

            if (Double.IsNegativeInfinity(result))
                result = Double.MinValue;

            if (Double.IsPositiveInfinity(result))
                result = Double.MaxValue;

            return result;
        }

        double EvaluateScore(DataValues[] training, DataValues[] feedback)
        {
            PolynomialFunction poly = new PolynomialFunction();
            poly.Train(training);
            poly.Predict(feedback);

            double score = DataGroup.GetScore(feedback);

            return score;
        }
        
        void DefineDynamicRegionFeedback(List<ExperimentDynResults> resultStats, DataGroup[] datagroups)
        {
            double statMinError1 = Double.MaxValue;
            double statMinError2 = Double.MaxValue;

            statMinError1 = resultStats.Min(r => r.Score1);
            statMinError2 = resultStats.Min(r => r.Score2);
            
            foreach (DataGroup dg in datagroups)
            {
                dg.ProvideFeedback(statMinError1, statMinError2);
            }

            ProvideFeedback(statMinError1, statMinError2);
        }

        public void DefineDynamicRegion()
        {   
            int iteractions = 10;

            DataGroup[] datagroups = new DataGroup[10];

            for (int i = 0; i < datagroups.Length; i++)
            {
                datagroups[i] = ResampledGroup();
            }

            for (int loop = 0; loop < iteractions; loop++)
            {
                List<ExperimentDynResults> resultStats = new List<ExperimentDynResults>();
                Debug.WriteLine("Dimension: {0} - {1}", this.Start, this.End);

                foreach (DataGroup dg in datagroups)
                {
                    DataValues[] trainingLow = dg.GetLowTrainingData();
                    DataValues[] feedback = dg.GetFeedbackData();

                    double scoreLow = EvaluateScore(trainingLow, feedback);

                    DataValues[] trainingHigh = dg.GetHighTrainingData();
                    DataValues[] prediction = dg.GetPredictionData();

                    double scoreHigh = EvaluateScore(trainingHigh, prediction);

                    resultStats.Add(new ExperimentDynResults { Score1 = Math.Log10(scoreLow), Score2 = Math.Log10(scoreHigh)});
                }

                DefineDynamicRegionFeedback(resultStats, datagroups);
            }

            Debug.WriteLine("Dimension: {0} - {1}", this.Start, this.End);
        }

        public DataGroup GetBestGroup()
        {
            return GetBestGroup((this.Start + this.End) / 2);
        }

        public DataGroup GetBestGroup(int dimension)
        {
            DataGroup bestDataGroup = null;
            double minScore = Double.MaxValue;

            List<ExperimentDynResults> finalResultStats = new List<ExperimentDynResults>();
            
            for (int i = 0; i < 100; i++) 
            {
                DataGroup dg = ResampledGroup(dimension);

                DataValues[] trainingLow = dg.GetMidTrainingData();
                DataValues[] feedback = dg.GetMidPredictionData();

                double score = EvaluateScore(trainingLow, feedback);

                if (score < minScore)
                {
                    bestDataGroup = dg;
                    minScore = Math.Log10(score);
                }

                finalResultStats.Add(new ExperimentDynResults { Score1 = Math.Log10(score) });
            }

            return bestDataGroup;
        }        
    }


}
