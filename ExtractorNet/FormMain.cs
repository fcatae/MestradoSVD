using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExtractorNet
{
    public partial class FormMain : Form
    {
        IDataSource m_reuters;

        public FormMain()
        {
            InitializeComponent();

            m_reuters = new Data.Reuters21578(@"c:\data\reuters\test3a");
                        
            //m_reuters = new Data.DataSample();

            // m_reuters = new GenericSelfDataSource(m_reuters);

            string filename = m_reuters.Name;

            this.Text = filename;

        }

        public FormMain(string foldername)
        {
            InitializeComponent();

            OpenReuters21578(foldername);
        }

        void OpenReuters21578(string foldername)
        {
            this.Text = foldername;
            m_reuters = new Data.Reuters21578(foldername);
        }

        static public FormMain CreateSessionAutomaticForm(string foldername)
        {
            FormMain form = new FormMain(foldername);

            form.Show();

            form.AutomateTests(null);

            return form;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            AddTask("CrossValidationTask", new CrossValidationTask(
                new ExperimentIdfVectorPredictor10(WordWeight.TFIDF)
                ));
            
            AddTask("ExperimentDVTFIDF", new ExperimentDVTFIDF());
            AddTask("Experiment1", new Experiment1(m_reuters));
                       
            AddTask("Simple Predictor", new ExperimentSimplePredictor());
            //AddTask("Vector", new ExperimentVectorPredictor());
            AddTask("Vector (min10)", new ExperimentWordFilterPredictor());
            //AddTask("Normalized Vector", new ExperimentNormalizedVectorPredictor());
            //AddTask("IDF Vector", new ExperimentIdfVectorPredictor());
            AddTask("IDF Vector (min10) TFIDF", new ExperimentIdfVectorPredictor10(WordWeight.TFIDF));
            AddTask("IDF Vector (min10) VTFIDF", new ExperimentIdfVectorPredictor10(WordWeight.VTFIDF));
            AddTask("IDF Normalized Vector (min10)", new ExperimentIDFNormalizedVectorPredictor10());
            AddTask("Dimension Predictor 1%", new ExperimentDimensionPredictor(.01));
            AddTask("Dimension Predictor 10%", new ExperimentDimensionPredictor(.10));
            AddTask("Dimension Predictor 20%", new ExperimentDimensionPredictor(.20));
            AddTask("Dimension Predictor 40%", new ExperimentDimensionPredictor(.40));
            AddTask("Dimension Predictor 80%", new ExperimentDimensionPredictor(.80));
            AddTask("Latent Semantic Analysis 100", new ExperimentLatentSemanticAnalysisPredictor(100));
            AddTask("Latent Semantic Analysis 200", new ExperimentLatentSemanticAnalysisPredictor(200));
            AddTask("Latent Semantic Analysis 400", new ExperimentLatentSemanticAnalysisPredictor(400));
            AddTask("Category Vectors 60%", new ExperimentCategoryVectorsPredictor(.60, 5));
            AddTask("Category Vectors Fixed 100", new ExperimentCategoryVectorsPredictor(0, 100));
            AddTask("Category Vectors Fixed Super 200", new ExperimentCategoryVectorsPredictor(0, 200));
            //AddTask("ExperimentTFIDF", new ExperimentTFIDF());
            //AddTask("Polynomial Enumeration (DebugOutput)", new ExperimentPolynomialReduction());
            //AddTask("Polynomial Reduction (DebugOutput)", new ExperimentDynamicPolysize());
            AddTask("LSA with Category Dimension 100", new ExperimentLsaCategoryPredictor(100));
            AddTask("LSA with Category Dimension 200", new ExperimentLsaCategoryPredictor(200));
            AddTask("LSA with Category Dimension 400", new ExperimentLsaCategoryPredictor(400));
            AddTask("LSA Projected Category 30", new ExperimentLsaProjCategoryPredictor(30));
            AddTask("LSA Projected Category 50", new ExperimentLsaProjCategoryPredictor(50));
            AddTask("LSA Projected Category 90", new ExperimentLsaProjCategoryPredictor(90));
            AddTask("LSA Projected Category 200", new ExperimentLsaProjCategoryPredictor(200));

            AddTask("", null);

            AddTask("Auto-adjust 1/3 (self test) Auto-predict", null);
            
            AddTask("Vector base with feedback WEIGHTING", null);

            //AddTask("HighScore: Dimension Predictor 80%", new ExperimentHighScore(.80));
            //IDataSource reuters = new GenericTwoDataSource(new Data.Reuters21578(@"c:\data\reuters\test2"));
            //IDF     = 91.9
            //Dim 20% = 92.4
            //Dim 80% = 94.7
            //Cat 60% = 77.6%
            //Cat 100 = 77.6%
        }

        private void AutomateTests(string[] tests)
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;

            foreach (ListViewItem item in listView1.Items)
            {
                if ( (tests==null) || (tests.Contains(item.Text)) )
                {
                    listView1.SelectedIndices.Clear();
                    item.Selected = true;

                    listView1_DoubleClick(this, new EventArgs());
                }
            }

            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = (listView1.SelectedItems.Count == 1) ? listView1.SelectedItems[0] : null;
            
            if ((item != null) & (item.Tag != null))
            {
                Debug.WriteLine("TASK: " + item.Text.ToString());
                ITask task = item.Tag as ITask;

                UpdateText(item, "Running");

                object result = ExecuteTask(task);
                
                UpdateText(item, "Running");

                if (result is double)
                    UpdateText(item, ((double)result).ToString("F1") + "%");
                else
                    UpdateText(item, result.ToString());
            }
        }

        void UpdateText(ListViewItem item, string text)
        {
            if (item.SubItems.Count == 1)
                item.SubItems.Add("");

            item.SubItems[1].Text = text;

            Application.DoEvents();
        }

        object ExecuteTask(ITask task)
        {
            object param = m_reuters;
            object result;

            DateTime startTime = DateTime.Now;
            Debug.WriteLine("Start time: {0}", startTime);
            result = task.Execute(param);
            Debug.WriteLine("Total Time Taken: {0} seconds", (DateTime.Now - startTime).TotalSeconds);

            return result;
        }


        object ExecuteSingleTask(ITask task)
        {
            object param = m_reuters;
            object result;

            DateTime startTime = DateTime.Now;
            Debug.WriteLine("Start time: {0}", startTime);
            result = task.Execute(param);
            Debug.WriteLine("Total Time Taken: {0} seconds", (DateTime.Now - startTime).TotalSeconds);

            return result;
        }

        object ExecuteTaskMultiple(ITask task)
        {
            object param = m_reuters;
            object result;

            DateTime startTime = DateTime.Now;
            Debug.WriteLine("Start time: {0}", startTime);
            
            result = CrossValidationTask.ExecuteTask(task, param);

            Debug.WriteLine("Total Time Taken: {0} seconds", (DateTime.Now - startTime).TotalSeconds);

            return result;
        }

        #region Interface
        
        public void AddTask(string name, ITask task)
        {
            ListViewItem listItem = new ListViewItem()
            {
                Text = name,
                Tag = task
            };

            listView1.Items.Add(listItem);
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            AutomateTests(null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ListViewItem item = (listView1.SelectedItems.Count == 1) ? listView1.SelectedItems[0] : null;

            if ((item != null) & (item.Tag != null))
            {
                Debug.WriteLine("TASK: " + item.Text.ToString());
                ITask task = item.Tag as ITask;

                UpdateText(item, "Running");

                object result = ExecuteTaskMultiple(task);

                UpdateText(item, "Running");

                if (result is double)
                    UpdateText(item, ((double)result).ToString("F1") + "%");
                else
                    UpdateText(item, result.ToString());
            }

        }
    }

}
