using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExperimentLatentSemanticAnalysisPredictor : ITask
    {
        private int m_dimensions;

        public ExperimentLatentSemanticAnalysisPredictor(int dimensions)
        {
            m_dimensions = dimensions;
        }

        public object Execute(object param)
        {
            Debug.Assert(param != null);

            double result = TaskPredictor.EvaluateScore(new LatentSemanticAnalysisPredictor(m_dimensions), (IDataSource)param);

            return result;
        }
    }

    [Obsolete]
    class LatentSemanticAnalysisPredictor : IPredictor
    {
        DocumentGroup m_allDocGroup = new DocumentGroup("all");
        MatrixSVD m_svd;

        private DocumentGroup[] m_trainingCategoryGroup;
        private IPredictor m_predictor;
        private int m_dimensions;
        private IPredictor m_vectorPredictor;

        public LatentSemanticAnalysisPredictor(int dimensions) : this(dimensions, 
            new IdfVectorPredictor(new VectorPredictor(new FilteredWordDictionary(10))))
        {
        }

        public LatentSemanticAnalysisPredictor(int dimensions, IPredictor predictor)
        {
            Debug.Assert(dimensions > 0);
            Debug.Assert(predictor != null);

            m_dimensions = dimensions;
            m_predictor = predictor;
        }

        public void Train(DocumentGroup[] trainingCategoryGroup)
        {
            foreach (DocumentGroup group in trainingCategoryGroup)
            {
                IncludeDocuments(group);
            }

            m_trainingCategoryGroup = trainingCategoryGroup;
        }

        public void PreProcess(DocumentGroup documentGroup)
        {
            IncludeDocuments(documentGroup);

            m_predictor.Train(new DocumentGroup[] { m_allDocGroup });

            ProcessLSA(m_dimensions);

            m_vectorPredictor = new VectorPredictor(null, true);
            m_vectorPredictor.Train(m_trainingCategoryGroup);
        }


        public string Predict(Document sampleDocument)
        {
            Debug.Assert( (sampleDocument.Vector != null) && (sampleDocument.Vector is SvdVector) );

            return m_vectorPredictor.Predict(sampleDocument);
        }

        void IncludeDocuments(DocumentGroup documentGroup)
        {
            Document[] documents = documentGroup.GetDocuments();

            //Debug.Assert(documents[0].Vector != null);

            foreach (Document doc in documents)
                m_allDocGroup.AddVirtual(doc);
        }

        void ProcessLSA(int dimension)
        {
            MatrixSVD svd = m_svd;

            foreach (Document doc in m_allDocGroup.GetDocuments())
                Debug.Assert(doc.Vector != null);

            if (m_svd == null)
            {
                svd = new MatrixSVD(m_allDocGroup);
                svd.Init();
                m_svd = svd;
            }

            svd.SetDimension(dimension);
            //double err = svd.GetApproximationError();
            
            foreach (Document doc in m_allDocGroup.GetDocuments())
                Debug.Assert(doc.Vector is SvdVector);
        }

    }

}
