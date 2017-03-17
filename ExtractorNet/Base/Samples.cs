using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractorNet.Samples
{
    class TaskSimple : ITask
    {
        private object m_value;

        public TaskSimple(object stored_value)
        {
            m_value = stored_value;
        }

        public object Execute(object param)
        {
            return m_value;
        }
    }
}
