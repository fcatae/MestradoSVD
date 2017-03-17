using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ExtractorNet.Data
{
    public class Reuters21578 : IDataSource
    {
        const string PATH_Reuters21578 = @"C:\Data\reuters";
        private string m_filename = null;
        private int m_statCountTrainingSet = 0;
        private int m_statCountTestSet = 0;
        private int m_statCountUnusedSet = 0;
        private int m_statCountOk = 0;
        private int m_statCountCategory0 = 0;
        private int m_statCountCategory1 = 0;
        private int m_statCountCategory2 = 0;
        private int m_statCountCategory3orMore = 0;
        private int m_statCountEmpty = 0;
        private int m_statCountEmptyTitle = 0;
        private int m_statCountEmptyBody = 0;
        private int m_statCountEmptyCategories = 0;
        private int m_statCountEmptyBlah = 0;
        private int m_statCountValidTrainingSet = 0;
        private int m_statCountValidTestSet = 0;

        private List<DataDocument> m_allDocuments = new List<DataDocument>();
        private List<DataDocument> m_testDocuments = new List<DataDocument>();
        private List<DataDocument> m_trainDocuments = new List<DataDocument>();

        public Reuters21578(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Extension.ToUpper() == ".SGM")
                {
                    OpenSgm(file.FullName);
                }
            }

            m_filename = path;
        }

        public Reuters21578()
            : this(PATH_Reuters21578)
        {

        }

        public string Name
        {
            get { return m_filename; }
        }

        void ShowStatistics()
        {
            Debug.WriteLine(" Training={0}, TestSet={1}", m_statCountValidTrainingSet, m_statCountValidTestSet);
        }

        private void OpenSgm(string filename)
        {
            Debug.WriteLine("Data.Reuters21578.OpenSGM(" + filename + ")");

            using (StreamReader reader = new StreamReader(filename))
            {
                // Remove the DTD
                reader.ReadLine();

                // Create the XmlReader (fragments)
                XmlReaderSettings settings = new XmlReaderSettings()
                {
                    ConformanceLevel = ConformanceLevel.Fragment,
                    CheckCharacters = false
                };

                XmlReader xmlread = XmlReader.Create(reader, settings);

                while (xmlread.ReadToFollowing("REUTERS"))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    XmlReader nr = xmlread.ReadSubtree();
                    DataDocument doc;

                    xmldoc.Load(nr);

                    doc = ProcessXmlDocument(xmldoc);

                    if (doc != null)
                    {
                        m_allDocuments.Add(doc);

                        if (doc.TestSet)
                            m_testDocuments.Add(doc);
                        else
                            m_trainDocuments.Add(doc);
                    }
                }
            }

            ShowStatistics();
        }

        private DataDocument ProcessXmlDocument(XmlDocument xmldoc)
        {
            DataDocument finalDocument = null;
            string modapte;
            string containtopics;
            string title;
            string body;
            string text;
            string[] categories;
            bool isTestSet = false;
            bool isTrainingSet = false;
            bool isUnusedSet = false;
            int numcat = 0;

            // ModApte
            XmlNode nodeLewisSplit = xmldoc.SelectSingleNode("/REUTERS/@LEWISSPLIT");
            XmlNode nodeContainsTopics = xmldoc.SelectSingleNode("/REUTERS/@TOPICS");

            Debug.Assert(nodeLewisSplit != null);
            Debug.Assert(nodeContainsTopics != null);

            modapte = nodeLewisSplit.Value;
            containtopics = nodeContainsTopics.Value;

            if (modapte == "TRAIN" && containtopics == "YES")
            {
                isTrainingSet = true;
                m_statCountTrainingSet++;
            }
            else if (modapte == "TEST" && containtopics == "YES")
            {
                isTestSet = true;
                m_statCountTestSet++;
            }
            else
            {
                isUnusedSet = true;
                m_statCountUnusedSet++;
            }

            if (isUnusedSet)
            {
                return null;
            }

            // XML
            XmlNode nodeText = xmldoc.SelectSingleNode("/REUTERS/TEXT");
            XmlNode nodeTitle = xmldoc.SelectSingleNode("/REUTERS/TEXT/TITLE");
            XmlNode nodeBody = xmldoc.SelectSingleNode("/REUTERS/TEXT/BODY");
            XmlNodeList nodeTopics = xmldoc.SelectNodes("/REUTERS/TOPICS/D");

            if ((nodeTitle == null) ||
                (nodeBody == null) ||
                (nodeTopics == null))
            {
                m_statCountEmpty++;

                if (nodeTitle == null) m_statCountEmptyTitle++;
                if (nodeBody == null) m_statCountEmptyBody++;
                if (nodeTopics == null) m_statCountEmptyCategories++;

                if (xmldoc.SelectSingleNode("/REUTERS/TEXT").InnerText.Contains("blah blah"))
                    m_statCountEmptyBlah++;
            }

            text = nodeText.InnerText;

            // Retrieve text data
            title = ( nodeTitle != null ) ? nodeTitle.InnerText : "";
            body = ( nodeBody != null ) ? nodeBody.InnerText : "";

            text = (title != "") ? title : text;
            text = (body != "") ? body : text;

            XmlNodeList topics = nodeTopics;

            categories = new string[topics.Count];

            foreach (XmlNode topicNode in topics)
            {
                categories[numcat] = topicNode.InnerText;

                numcat++;
            }

            // Statistics
            m_statCountOk++;

            if (numcat == 0)
                m_statCountCategory0++;
            else if (numcat == 1)
                m_statCountCategory1++;
            else if (numcat == 2)
                m_statCountCategory2++;
            else if (numcat >= 3)
                m_statCountCategory3orMore++;
                                 
            if (isTrainingSet && (numcat>0) && (body!=""))
            {
                m_statCountValidTrainingSet++;
            }

            if (isTestSet && (numcat>0) && (body!=""))
            {
                m_statCountValidTestSet++;
            }

            if( categories.Length == 1 )
            {
                finalDocument = new DataDocument(text, categories[0]) 
                    { TestSet = isTestSet };
            }

            if (body == "")
                finalDocument = null;

            return finalDocument;
        }

        public IEnumerable<DataDocument> GetTrainingSet()
        {
            return m_trainDocuments;
        }
        
        public IEnumerable<DataDocument> GetDocuments()
        {
            return m_testDocuments;
        }
    }


}
