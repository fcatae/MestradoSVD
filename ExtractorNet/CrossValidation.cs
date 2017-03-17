using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class CrossValidationTask : ITask
    {
        const int TotalTests = 5;

        ITask m_task;

        public CrossValidationTask(ITask task)
        {
            m_task = task;
        }

        public object Execute(object param)
        {
            return ExecuteTask(m_task, param);
        }

        public static object ExecuteTask(ITask task, object param)
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;

            IDataSource datasource = (IDataSource)param;

            FormShowResults form = new FormShowResults();

            form.Show();

            CrossValidationDataSource crossData = new CrossValidationDataSource(datasource);

            Debug.WriteLine(" Cross-Validation: " + datasource.Name);

            object[] resultSet = new object[CrossValidationTask.TotalTests];

            for (int i = 0; i < CrossValidationTask.TotalTests; i++)
            {
                DateTime startTime = DateTime.Now;

                IDataSource ds = crossData.CreateDataSource(i);

                object result = task.Execute(ds);

                Debug.WriteLine("  Step {0} - Total Time Taken: {1:F3} seconds", i, (DateTime.Now - startTime).TotalSeconds);

                form.AddTask(ds.Name, result);

                resultSet[i] = result;
            }

            form.Finished();

            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return resultSet[0];
        }

    
        
        void TestCrossValidation()
        {
            IDataSource datasource = new Data.DataAlphaBeta();
            CrossValidationDataSource crossData = new CrossValidationDataSource(datasource);

            for (int i = 0; i < 10; i++)
            {
                IDataSource ds = crossData.CreateDataSource(i);

                Debug.WriteLine(ds.Name);

                Debug.Write("Training Set: \t\t\t\t|");
                foreach (DataDocument doc in ds.GetTrainingSet())
                {
                    Debug.Write(doc.Text);
                    Debug.Write("|");
                }
                Debug.WriteLine("");
                Debug.Write("Final Document Test Set: \t|");
                foreach (DataDocument doc in ds.GetDocuments())
                {
                    Debug.Write(doc.Text);
                    Debug.Write("|");
                }
                Debug.WriteLine("");
                Debug.WriteLine("");
            }
        }
        
    }

    class CrossValidationDataSource
    {
        public class DataSource : IDataSource
        {
            DataDocument[] m_training;
            DataDocument[] m_documents;

            public DataSource(string name, DataDocument[] training, DataDocument[] documents)
            {
                this.Name = name;
                this.m_training = training;
                this.m_documents = documents;
            }

            public string Name { get; private set; }

            public IEnumerable<DataDocument> GetTrainingSet()
            {
                return m_training;
            }

            public IEnumerable<DataDocument> GetDocuments()
            {
                return m_documents;
            }
        }

        DataDocument[] m_dataDocuments;
        string m_name;
        int m_trainingSize;
        int m_testSize;

        public CrossValidationDataSource(IDataSource datasource)
        {
            List<DataDocument> documentList = new List<DataDocument>();
            int trainingSize;
            int testSize;

            documentList.AddRange(datasource.GetTrainingSet());
            trainingSize = documentList.Count();

            documentList.AddRange(datasource.GetDocuments());
            testSize = documentList.Count() - trainingSize;

            Debug.Assert(trainingSize > 0);
            Debug.Assert(testSize > 0);
            Debug.Assert(trainingSize + testSize == documentList.Count);

            m_name = datasource.Name;
            m_trainingSize = trainingSize;
            m_testSize = testSize;
            m_dataDocuments = documentList.ToArray();
        }

        public CrossValidationDataSource(string name, DataDocument[] documentArray, int trainingSize)
        {
            List<DataDocument> documentList = new List<DataDocument>();
            int testSize;

            documentList.AddRange(documentArray);
            testSize = documentList.Count() - trainingSize;

            Debug.Assert(trainingSize > 0);
            Debug.Assert(testSize > 0);
            Debug.Assert(trainingSize + testSize == documentList.Count);

            m_name = name;
            m_trainingSize = trainingSize;
            m_testSize = testSize;
            m_dataDocuments = documentList.ToArray();
        }


        public CrossValidationDataSource(string name, DataDocument[] documentArray)
        {
            List<DataDocument> documentList = new List<DataDocument>();
            int trainingSize;
            int testSize;

            documentList.AddRange(documentArray);
            trainingSize = documentList.Count() / 2;
            testSize = documentList.Count() - trainingSize;

            Debug.Assert(trainingSize > 0);
            Debug.Assert(testSize > 0);
            Debug.Assert(trainingSize + testSize == documentList.Count);

            m_name = name;
            m_trainingSize = trainingSize;
            m_testSize = testSize;
            m_dataDocuments = documentList.ToArray();
        }


        public IDataSource CreateDataSource(int number)
        {
            DataDocument[] randomDocumentArray = CreateRandomDocumentArray(number);

            string name = String.Format("{0}({1:000})", m_name, number);
            DataDocument[] docs1 = new DataDocument[m_trainingSize];
            DataDocument[] docs2 = new DataDocument[m_testSize];

            Array.Copy(randomDocumentArray, docs1, docs1.Length);
            Array.Copy(randomDocumentArray, docs1.Length, docs2, 0, docs2.Length);

            return new DataSource(name, docs1, docs2); ;
        }

        public IDataSource CreateDataSource(int number, int trainingSize)
        {
            DataDocument[] randomDocumentArray = CreateRandomDocumentArray(number);
            int totalSize = randomDocumentArray.Length;

            string name = String.Format("{0}({1:000})", m_name, number);
            DataDocument[] docs1 = new DataDocument[trainingSize];
            DataDocument[] docs2 = new DataDocument[totalSize - trainingSize];

            Array.Copy(randomDocumentArray, docs1, docs1.Length);
            Array.Copy(randomDocumentArray, docs1.Length, docs2, 0, docs2.Length);

            return new DataSource(name, docs1, docs2); ;
        }

        DataDocument[] CreateRandomDocumentArray(int number)
        {
            Random random = new Random(number);

            DataDocument[] array = (DataDocument[])m_dataDocuments.Clone();

            if (number != 0)
            {
                for (int i = array.Length; i > 1; i--)
                {
                    // Pick random element to swap.
                    int j = random.Next(i); // 0 <= j <= i-1
                    // Swap.
                    DataDocument tmp = array[j];
                    array[j] = array[i - 1];
                    array[i - 1] = tmp;
                }
            }

            return array;
        }        
    }
}
