using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractorNet.Data
{

    class DataAlphaBeta : IDataSource
    {
        static readonly string[] TextData = {
            "0 aaa", "0 bbb", "0 ccc", "0 ddd", "0 eee", "0 fff", "0 ggg", "0 hhh", "0 iii", "0 jjj", 
            "1 kk", "1 ll", "1 mm", "1 nn", "1 oo" };

        private static readonly List<DataDocument> g_training;
        private static readonly List<DataDocument> g_testset;

        static DataAlphaBeta()
        {
            g_training = new List<DataDocument>();
            g_testset = new List<DataDocument>();

            foreach (string title in TextData)
            {
                char category = title[0];
                string text = title.Substring(2);

                List<DataDocument> list = (category == '0') ? g_training : g_testset;

                list.Add(new DataDocument(text, ""));
            }
        }

        public string Name
        {
            get { return "DataAlphaBeta"; }
        }

        public IEnumerable<DataDocument> GetTrainingSet()
        {
            return g_training;
        }

        public IEnumerable<DataDocument> GetDocuments()
        {
            return g_testset;
        }
    }


    class DataRepeat : IDataSource
    {
        static readonly string[] Titles = {
            "z0: abc def z xxx", 
            "a1: abc abc abc abc abc z z z z z z z z z z xxx", 
            "d1: def def def def def z z z z z z z z z z xxx",
            "a2: abc abc abc abc abc abc abc abc abc abc z z z z z z z z z z",
            "d2: def def def def def def def def def def z z z z z z z z z z",
            "z1: abc abc abc abc abc def def def def def z z z z z z z z z z"};

        private static readonly List<DataDocument> g_documents;

        private readonly List<DataDocument> m_training;
        private readonly List<DataDocument> m_testset;

        static DataRepeat()
        {
            g_documents = new List<DataDocument>();

            foreach (string title in Titles)
            {
                string category = new string(title[0], 1);
                string text = title.Substring(4);

                g_documents.Add(new DataDocument(text, category));
            }
        }

        public DataRepeat()
        {
            m_training = new List<DataDocument>();
            m_testset = new List<DataDocument>();

            m_training.Add(g_documents[0]);
            m_training.Add(g_documents[1]);
            m_training.Add(g_documents[2]);
            m_training.Add(g_documents[3]);
            m_training.Add(g_documents[4]);
            m_testset.Add(g_documents[5]);
        }

        public string Name
        {
            get { return "DataRepeat"; }
        }

        public IEnumerable<DataDocument> GetTrainingSet()
        {
            return m_training;
        }

        public IEnumerable<DataDocument> GetDocuments()
        {
            return m_testset;
        }
    }

    class DataSample : IDataSource
    {
        static readonly string[] Titles = {
            "c1: Human machine interface for ABC computer applications", 
            "c2: A survey of user opinion of computer system response time",
            "c3: The EPS user interface management system",
            "c4: System and human system engineering testing of EPS",
            "c5: Relation of user perceived response time to error measurement",
            "m1: The generation of random, binary, ordered trees",
            "m2: The intersection graph of paths in trees",
            "m3: Graph minors IV: Widths of trees and well-quasi-ordering",
            "m4: Graph minors: A survey"};

        private static readonly List<DataDocument> g_documents;

        private readonly List<DataDocument> m_training;
        private readonly List<DataDocument> m_testset;

        static DataSample()
        {
            g_documents = new List<DataDocument>();

            foreach (string title in Titles)
            {
                string category = new string(title[0], 1);
                string text = title.Substring(4);

                g_documents.Add(new DataDocument(text, category));
            }
        }

        public DataSample()
        {
            m_training = new List<DataDocument>();
            m_testset = new List<DataDocument>();

            m_training.Add(g_documents[0]);
            m_testset.Add(g_documents[1]);
            m_training.Add(g_documents[2]);
            m_training.Add(g_documents[3]);
            m_testset.Add(g_documents[4]);
            m_training.Add(g_documents[5]);
            m_testset.Add(g_documents[6]);
            m_testset.Add(g_documents[7]);
            m_training.Add(g_documents[8]);
        }

        public string Name
        {
            get { return "DataSample"; } 
        }

        public IEnumerable<DataDocument> GetTrainingSet()
        {
            return m_training;
        }

        public IEnumerable<DataDocument> GetDocuments()
        {
            return m_testset;
        }
    }
}
