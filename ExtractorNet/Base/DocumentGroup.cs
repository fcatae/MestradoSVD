using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class DocumentGroup
    {
        private string m_name;
        private List<DataDocument> m_catdocs = new List<DataDocument>();
        private List<Document> m_docs = new List<Document>();

        public static int CountCategories(IDataSource dataSource)
        {
            DocumentGroup[] groups = GroupCategories(dataSource);

            return groups.Length;
        }

        public static DocumentGroup GetTrainingData(IDataSource dataSource)
        {
            DocumentGroup group = new DocumentGroup("Training");

            var testset = dataSource.GetTrainingSet();

            foreach (DataDocument doc in testset)
            {
                group.Add(doc);
            }

            return group;
        }

        public static DocumentGroup GetFinalData(IDataSource dataSource)
        {
            DocumentGroup group = new DocumentGroup("TestSet");
            
            var testset = dataSource.GetDocuments();

            foreach (DataDocument doc in testset)
            {
                group.Add(doc);
            }

            return group;
        }

        public static DocumentGroup[] GroupCategories(IDataSource dataSource)
        {
            Dictionary<string, DocumentGroup> categoryList = new Dictionary<string, DocumentGroup>();

            var trainingSet = dataSource.GetTrainingSet();

            foreach (DataDocument doc in trainingSet)
            {
                string category = doc.Category;

                if (!categoryList.Keys.Contains(category))
                    categoryList.Add(category, new DocumentGroup(category));

                categoryList[category].Add(doc);
            }

            return categoryList.Values.ToArray();
        }

        public DocumentGroup(string name)
        {
            m_name = name;
        }

        public string Name
        {
            get { return m_name; }
        }

        public void Add(DataDocument doc)
        {
            m_catdocs.Add(doc);
            m_docs.Add(new Document(doc, doc.Category));
        }

        public void AddVirtual(Document doc)
        {
            // it may cause real trouble 
            m_catdocs = null;

            m_docs.Add(doc);
        }

        public DataDocument[] GetCatDocuments()
        {
            return m_catdocs.ToArray();
        }


        public Document[] GetDocuments()
        {
            return m_docs.ToArray();
        }
    }
}
