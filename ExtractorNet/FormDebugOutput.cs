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
    public partial class FormDebugOutput : Form
    {
        public FormDebugOutput()
        {
            InitializeComponent();
        }

        public void SetText(string text)
        {
            textBox1.Text = text;
        }
    }

    public class FormDebugOutputTraceListener : TraceListener
    {
        FormDebugOutput m_form = new FormDebugOutput();
        StringBuilder m_strBuilder = new StringBuilder();

        public FormDebugOutputTraceListener(string name) : base(name)
        {
        }

        public override void Write(string text)
        {
            m_strBuilder.Append(text);
        }
        public override void WriteLine(string text)
        {
            m_strBuilder.AppendLine(text);
        }

        protected void Show()
        {
            FormDebugOutput form = new FormDebugOutput();

            form.SetText( m_strBuilder.ToString() );

            form.Show();
        }

        public static void Start()
        {
            Debug.Listeners.Add(new FormDebugOutputTraceListener("FormDebugOutputTraceListener"));
        }

        public static void End()
        {
            FormDebugOutputTraceListener listener = (FormDebugOutputTraceListener)Debug.Listeners["FormDebugOutputTraceListener"];

            listener.Show();

            Debug.Listeners.Remove(listener);
        }

    } 

}
