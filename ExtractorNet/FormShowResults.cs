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
    public partial class FormShowResults : Form
    {
        private bool m_preventClosing = true;

        public FormShowResults()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Text = "Running...";
        }

        public void AddTask(string name, object value)
        {
            ListViewItem listItem = new ListViewItem();

            string result = (value is double) ? ((double)value).ToString("F2") : value.ToString();

            listItem.Text = name;
            listItem.SubItems.Add(result);

            listView1.Items.Add(listItem);

            Application.DoEvents();
        }

        public void Finished()
        {
            this.Text = "Task Results";
            m_preventClosing = false;
        }

        private void FormShowResults_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = m_preventClosing;
        }

    }

}
