using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    public interface IDataSource
    {
        string Name { get; }
        IEnumerable<DataDocument> GetTrainingSet();
        IEnumerable<DataDocument> GetDocuments();
    }

    public class GenericDataSource : IDataSource
    {
        IDataSource m_datasource;

        public string Name
        {
            get { return m_datasource.Name; }
        }

        public GenericDataSource(IDataSource datasource)
        {
            m_datasource = datasource;
        }
        
        public IEnumerable<DataDocument> GetTrainingSet()
        {
            return m_datasource.GetTrainingSet();
        }

        public IEnumerable<DataDocument> GetDocuments()
        {
            return m_datasource.GetDocuments();
        }
    }

    public class GenericSelfDataSource : IDataSource
    {
        IDataSource m_datasource;

        public string Name
        {
            get { return m_datasource.Name + " (Self)"; }
        }

        public GenericSelfDataSource(IDataSource datasource)
        {
            m_datasource = datasource;
        }

        public IEnumerable<DataDocument> GetTrainingSet()
        {
            return m_datasource.GetTrainingSet();
        }

        public IEnumerable<DataDocument> GetDocuments()
        {
            return m_datasource.GetTrainingSet();
        }
    }

    public class GenericTwoDataSource : IDataSource
    {
        string m_dataName;

        List<DataDocument> m_trainingSet = new List<DataDocument>();
        List<DataDocument> m_finalTestDocuments = new List<DataDocument>();

        public string Name
        {
            get { return m_dataName + " (2 Categories)"; }
        }

        public GenericTwoDataSource(IDataSource datasource)
        {
            m_dataName = datasource.Name;

            foreach (DataDocument train in datasource.GetTrainingSet())
            {
                if( train.Category == "earn" || train.Category == "acq" )
                    m_trainingSet.Add(train);
            }

            foreach (DataDocument doc in datasource.GetDocuments())
            {
                if( doc.Category == "earn" || doc.Category == "acq" )
                    m_finalTestDocuments.Add(doc);
            }
        }

        public IEnumerable<DataDocument> GetTrainingSet()
        {
            return m_trainingSet;
        }

        public IEnumerable<DataDocument> GetDocuments()
        {
            return m_finalTestDocuments;
        }
    }


}
