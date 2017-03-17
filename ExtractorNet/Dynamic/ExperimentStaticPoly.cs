using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    interface IExperimentTraining
    {
        void Train(ITrainingData[] data);
        void Predict(IClassifyData[] data);
    }

    interface ITrainingData
    {
        double x { get; }
        double Value { get; }
    }

    interface IClassifyData
    {
        double x { get; }
        void SetValue(double value);
        void Reset();
    }

    class DataValues : ITrainingData, IClassifyData
    {
        protected readonly double m_x;
        protected readonly double m_originalValue;
        protected double m_predictedValue;

        public double x { get { return m_x; } }
        public double Value { get { return m_originalValue; } }
        public double PredictedValue
        {
            get { return m_predictedValue; }
            set { m_predictedValue=value; } 
        }

        public DataValues(double x, double value)
        {
            Debug.Assert(!Double.IsNaN(x));
            Debug.Assert(!Double.IsNaN(value));

            this.m_x = x;
            this.m_predictedValue = Double.NaN;
            this.m_originalValue = value;
        }

        public double Error
        {
            get
            {
                double error;

                Debug.Assert(!Double.IsNaN(m_predictedValue), "Still need to supply the predicted value");

                error = Math.Pow(this.m_originalValue - this.m_predictedValue, 2);

                //if (error > 100)
                //{
                //    if (Math.Abs(this.m_originalValue) > 1)
                //        error = Math.Pow((this.m_originalValue - this.m_predictedValue) / this.m_originalValue, 2);

                //    if (error > 100)
                //        error = 100;
                //    else
                //        error = 0;
                //}

                Debug.Assert(!Double.IsNaN(error));
                Debug.Assert(!Double.IsInfinity(error));

                return error;
            }
        }

        public void SetValue(double value)
        {
            m_predictedValue = value;
        }

        public void Reset()
        {
            m_predictedValue = Double.NaN;
        }

    }
    
    class ExperimentPolynomialReduction : ITask
    {
        public static Func<double, double> f = PolynomialFunction.f20;

        class ExperimentResult
        {
            public int dimension;
            public double error;
        }

        // Testar a melhoria de polinomio
        public object Execute(object param)
        {
            List<ExperimentResult> results = new List<ExperimentResult>();

            FormDebugOutputTraceListener.Start();

            for (int i = 1; i < 100; i++)
            {
                double error = CreateScenario(i, 100);

                results.Add(new ExperimentResult() { dimension = i, error = error });

                Debug.WriteLine("#samples={0:D2}: score={1:F1} ({2:F5})", i, Math.Log10(error), error);
            }

            double min_error = results.Min(r => r.error);

            foreach (ExperimentResult resultLine in results)
            {
                if( resultLine.error == min_error )
                    Debug.WriteLine("-- Best sample ({0:D2}): score=({1:F5}), log={2:f1}",
                        resultLine.dimension, resultLine.error, Math.Log10(resultLine.error));
            }

            FormDebugOutputTraceListener.End();

            return min_error;
        }

        double CreateScenario(int train, int prediction)
        {
            DataValues[] training = PolynomialFunction.CreateDataSet(f, train);
            DataValues[] feedback = PolynomialFunction.CreateDataSet(f, prediction);
            
            PolynomialFunction poly = new PolynomialFunction();
            double error;

            //poly.ValidationTest(training);

            poly.Train(training);
            poly.Predict(feedback);

            error = DataGroup.GetScore(feedback);

            return error;
        }


    }

    class PolynomialFunction : IExperimentTraining
    {
        public static Func<double, double> f1 = (x) => (x + 1.0);
        public static Func<double, double> f2 = (x) => (x * x + .2 * Math.Sin(x));
        public static Func<double, double> f4 = (x) => (10 * Math.Pow((x - 500.0) / 500.0, 6) +
                                                5 * Math.Pow((x - 500.0) / 500.0, 5)
                                                - 200 * Math.Pow((x - 500.0) / 500.0, 4) +
                                            Math.Pow((x - 500.0) / 500.0, 3) + 
                                            Math.Pow(x/13, 2) + Math.Sin(x));
        public static Func<double, double> f20 = (x) => (Math.Pow(x / 100.0, 20));
        public static Func<double, double> f30 = (x) => (50 * Math.Pow((x - 500.0) / 500.0, 31)
                                                - 200 * Math.Pow((x - 500.0) / 500.0, 20) +
                                            Math.Pow((x - 500.0) / 500.0, 3) + .2 * Math.Sin(x));
        public static Func<double, double> f30s = (x) => (50 * Math.Pow((x - 500.0) / 500.0, 31)
                                                - 200 * Math.Pow((x - 500.0) / 500.0, 20) +
                                            Math.Pow((x - 500.0) / 500.0, 3) + .2 * Math.Sin(x / 100.0));
        public static Func<double, double> f50 = (x) => (10 * Math.Pow((x - 500.0) / 500.0, 51) +
                                                5 * Math.Pow((x - 500.0) / 500.0, 31)
                                                - 200 * Math.Pow((x - 500.0) / 500.0, 20) +
                                            Math.Pow((x - 500.0) / 500.0, 3) + .2 * Math.Sin(x));
        public static Func<double, double> f50s = (x) => (10 * Math.Pow((x - 500.0) / 500.0, 51) +
                                                5 * Math.Pow((x - 500.0) / 500.0, 31)
                                                - 200 * Math.Pow((x - 500.0) / 500.0, 20) +
                                            Math.Pow((x - 500.0) / 500.0, 3) + .2 * Math.Sin(x/100.0));
        public static Func<double, double> f_sin = (x) => (x * x * Math.Sin(x / 500.0));
        public static Func<double, double> f_sqrt = (x) => (Math.Sqrt(Math.Abs(x)));


        private static Random g_random = new Random(0);

        ITrainingData[] m_values;
        double[] m_kValues;
        bool m_isSelfTesting = false;
        

        public static DataValues[] CreateDataSet(Func<double, double> func, int samples)
        {
            Debug.Assert(samples > 0);

            DataValues[] dataValues = new DataValues[samples];

            for (int i = 0; i < samples; i++)
            {
                double x = (1.0 - 2 * g_random.NextDouble()) * 1000;
                double fx = func(x);

                dataValues[i] = new DataValues(x, fx);
            }

            return dataValues;
        }

        public void Train(ITrainingData[] data)
        {
            Debug.Assert(data.Length > 0);

            m_kValues = CalculateK(data);
            m_values = data;
        }

        static double CalculateComponent(ITrainingData[] values, int comp, double x)
        {
            // F = k0(x-1)(x-2)...(x-n) + k1(x-0)(x-2)..(x-n) + k2()()...
            //  k= Value / ( (x-1)(x-2)...(x-n) )
            double prod = 1.0;

            for (int i = 0; i < values.Length; i++)
            {
                ITrainingData val = values[i];

                if (i != comp)
                {
                    prod *= (i != comp) ? (x - val.x) : 1.0;
                }
            }

            return prod;
        }

        static double[] CalculateK(ITrainingData[] datavalues)
        {
            Debug.Assert((datavalues != null) && (datavalues.Length > 0));

            double[] kValues = new double[datavalues.Length];

            // F = k0(x-1)(x-2)...(x-n) + k1(x-0)(x-2)..(x-n) + k2()()...
            //  k= Value / ( (x-1)(x-2)...(x-n) )
            for (int i = 0; i < datavalues.Length; i++)
            {
                ITrainingData data = datavalues[i];
                double comp = CalculateComponent(datavalues, i, data.x);
                double k = data.Value / comp;

                Debug.Assert(!Double.IsNaN(k));

                kValues[i] = k;
            }

            return kValues;
        }

        double Function(double x)
        {
            // Use k-cached values
            ITrainingData[] values = m_values;
            double[] kValues = m_kValues;
            double sum = 0;

            if (values.Length == 1)
            {
                // F = const
                sum = values[0].Value;
            }
            else
            {
                // F = k0(x-1)(x-2)...(x-n) + k1(x-0)(x-2)..(x-n) + k2()()...
                for (int i = 0; i < values.Length; i++)
                {
                    double comp = CalculateComponent(values, i, x);
                    double k = kValues[i];

                    Debug.Assert(!Double.IsNaN(comp));
                    Debug.Assert(!Double.IsNaN(k));

                    if (m_isSelfTesting)
                    {
                        // During a self-testing, sum = 0.0 (no previous comp)
                        if (comp != 0.0)
                        {
                            Debug.Assert(sum == 0.0);

                            double value_actual = values[i].Value;
                            double value_estimated = k * comp;

                            double error = Math.Abs(value_actual - value_estimated);

                            Debug.Assert(error < 1e-5);
                        }

                    }

                    sum += k * comp;
                    Debug.Assert(!Double.IsNaN(sum));
                }
            }

            Debug.Assert(!Double.IsNaN(sum));

            return sum;
        }

        public void Predict(IClassifyData[] dataValues)
        {
            foreach (IClassifyData data in dataValues)
            {
                double x = data.x;
                double val = Function(x);
                data.SetValue(val);

                ValidateSelfTestingPrediction(data);
            }
        }

        [Conditional("DEBUG")]
        void ValidateSelfTestingPrediction(object o)
        {
            DataValues data = o as DataValues;
            Debug.Assert((!m_isSelfTesting) || data.Error < 1e-10);
        }

        public void ValidationTest(DataValues[] dataValues)
        {
            m_isSelfTesting = true;

            Train(dataValues);
            Predict(dataValues);

            foreach (DataValues data in dataValues)
            {
                Debug.Assert(data.Error < 1e-10);
            }

            m_isSelfTesting = false;
        }
    }

}
