using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractorNet
{
    class ExternalControl
    {
        // recebe os dados do data source
        public static void Control(IDataSource datasource)
        {
            List<DataDocument> training = new List<DataDocument>();

            training.AddRange(datasource.GetTrainingSet());
        }

    }
}
