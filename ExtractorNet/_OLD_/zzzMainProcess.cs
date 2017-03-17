//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Xml;
//using System.Xml.XPath;

//namespace ExtractorNet
//{
//    class MainProcess
//    {
//        public static void Main()
//        {
//            //Data.Reuters21578 reuters = new Data.Reuters21578();
//            Data.Reuters21578 reuters = new Data.Reuters21578(@"c:\data\reuters\test");
            
//            SmartDude smartGuy = new SmartCategory();

//            //SelfAssessment(reuters, smartGuy);
//            AssessmentTwoGroups(reuters, smartGuy);
//        }

//        static void SelfAssessment(Data.Reuters21578 reuters, SmartDude smartGuy)
//        {
//            List<DataDocument> prepDocs1 = new List<DataDocument>(reuters.GetTrainingSet());
//            List<DataDocument> prepDocs2 = new List<DataDocument>(reuters.GetDocuments());

//            smartGuy.Prepare(prepDocs1, prepDocs1);

//            // training
//            foreach (DataDocument train in reuters.GetTrainingSet())
//            {
//                smartGuy.Train(train);
//            }

//            int statGood = 0;
//            int statWrong = 0;
//            int statTotal = 0;

//            // classification
//            //foreach (Doc test in reuters.GetTestDocuments())
//            foreach (DataDocument test in reuters.GetTrainingSet())
//            {
//                string classification = smartGuy.Classify(test);

//                statTotal++;
//                if (classification == test.Category)
//                {
//                    statGood++;
//                }
//                else
//                {
//                    statWrong++;
//                }
//            }

//            Debug.WriteLine("Results: Correct={0} ({1} out of {2})", (1.0 * statGood) / statTotal, statGood, statTotal); 
//        }

//        static void AssessmentTwoGroups(Data.Reuters21578 reuters, SmartDude smartGuy)
//        {
//            List<DataDocument> prepDocs1 = new List<DataDocument>(reuters.GetTrainingSet());
//            List<DataDocument> prepDocs2 = new List<DataDocument>(reuters.GetDocuments());

//            smartGuy.Prepare(prepDocs1, prepDocs2);

//            // training
//            foreach (DataDocument train in reuters.GetTrainingSet())
//            {
//                if( train.Category == "earn" || train.Category == "acq" )
//                    smartGuy.Train(train);
//            }

//            int statGood = 0;
//            int statWrong = 0;
//            int statTotal = 0;

//            // classification
//            //foreach (Doc test in reuters.GetTestDocuments())
//            foreach (DataDocument test in reuters.GetDocuments())
//            {
//                if (test.Category == "earn" || test.Category == "acq")
//                {
//                    string classification = smartGuy.Classify(test);

//                    statTotal++;
//                    if (classification == test.Category)
//                    {
//                        statGood++;
//                    }
//                    else
//                    {
//                        statWrong++;
//                    }
//                }
//            }

//            Debug.WriteLine("Results: Correct={0} ({1} out of {2})", (1.0 * statGood) / statTotal, statGood, statTotal);
//        }

//        static void Assessment(Data.Reuters21578 reuters, SmartDude smartGuy)
//        {
//            List<DataDocument> prepDocs1 = new List<DataDocument>(reuters.GetTrainingSet());
//            List<DataDocument> prepDocs2 = new List<DataDocument>(reuters.GetDocuments());

//            smartGuy.Prepare(prepDocs1, prepDocs2);

//            // training
//            foreach (DataDocument train in reuters.GetTrainingSet())
//            {
//                smartGuy.Train(train);
//            }

//            int statGood = 0;
//            int statWrong = 0;
//            int statTotal = 0;

//            // classification
//            //foreach (Doc test in reuters.GetTestDocuments())
//            foreach (DataDocument test in reuters.GetDocuments())
//            {
//                string classification = smartGuy.Classify(test);

//                statTotal++;
//                if (classification == test.Category)
//                {
//                    statGood++;
//                }
//                else
//                {
//                    statWrong++;
//                }
//            }

//            Debug.WriteLine("Results: Correct={0} ({1} out of {2})", (1.0 * statGood) / statTotal, statGood, statTotal); 
//        }
//    }
//}
