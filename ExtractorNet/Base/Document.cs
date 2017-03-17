using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    public class TextDocument
    {
        protected string m_text;

        public string Text
        {
            get { return m_text; }
        }

        public TextDocument(string text)
        {
            m_text = text;
        }
    }

    public class DataDocument : TextDocument
    {
        protected string m_category;
        public bool TestSet;
        public object CachedObj = null;
        //public VectorInitial CachedVector = null;

        public string Category
        {
            get { return m_category; }
        }

        public DataDocument(string text, string category) : base(text)
        {
            m_category = category;

            this.TestSet = false;
        }
    }

    class GenericDocument
    {
        private object m_document;

        protected object _value
        {
            get { return m_document; }
        }

        public GenericDocument(object doc)
        {
            Debug.Assert(doc != null);

            m_document = doc;
        }

        public TextDocument TextDocument
        {
            get { return m_document as TextDocument; }
        }

        public Bow Bow
        {
            get { return m_document as Bow; }
        }

        public Vector Vector
        {
            get { return m_document as Vector; }
        }

        public void Set(Bow doc)
        {
            Debug.Assert(doc != null);
            m_document = doc;
        }

        public void Set(Vector doc)
        {
            Debug.Assert(doc != null);
            m_document = doc;
        }
    }

    class GenericDocumentClassified : GenericDocument
    {
        private string m_category;

        public GenericDocumentClassified(TextDocument doc, string category) : base(doc)
        {
            m_category = category;
        }

        public GenericDocument GetTextDocument()
        {
            return new GenericDocument(this._value);
        }
    }

    class GenericDocumentMultipleCategories : GenericDocument
    {
        private string[] m_categories;

        public GenericDocumentMultipleCategories(object doc, string[] categories)
            : base(doc)
        {
            m_categories = categories;
        }
    }

    class Document
    {
        private object m_document;
        private string m_category;

        public object Value
        {
            get { return m_document; }
        }

        public string Category
        {
            get { return m_category; }
        }

        public Document(object doc, string category)
        {
            Debug.Assert(doc != null);

            m_document = doc;
            m_category = category;
        }

        public TextDocument TextDocument
        {
            get { return m_document as TextDocument; }
        }

        public Bow Bow
        {
            get { return m_document as Bow; }
        }

        public Vector Vector
        {
            get { return m_document as Vector; }
        }

        public void Set(Bow doc)
        {
            Debug.Assert(doc != null);
            m_document = doc;
        }

        public void Set(Vector doc)
        {
            Debug.Assert(doc != null);
            m_document = doc;
        }

    }
}
