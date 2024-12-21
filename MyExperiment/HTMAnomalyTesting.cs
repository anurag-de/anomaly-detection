using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyExperiment
{
    /// <summary>
    /// This class is responsible for testing an HTM model.
    /// CSV files from both training(learning) and predicting folders will be used for training our HTM Model.
    /// Default training(learning) and predicting folder paths are passed on to the constructor.
    /// Testing is carried out by trained model created using HTMModeltraining class,
    /// then CSVFolderReader is used to read all the sequences from all the CSV files inside predicting folder and trimmed,
    /// after that, the trimmed subsequences are tested sequence by sequence. 
    /// In the end, DetectAnomaly method will be used for
    /// testing all the elements in a sequence as sliding window, one by one.
    /// </summary>
    public class HTMAnomalyTesting
    {
        // Class level variables declared for storing input/predicting (testing) folder paths
        // and for calculating accuracy for anomaly testing        
        private readonly string _trainingFolderPath;
        private readonly string _predictingFolderPath;
        private readonly string _defaultTrainingFolderPath;
        private readonly string _defaultPredictingFolderPath;
        private static double totalAccuracy = 0.0;
        private static int iterationCount = 0;

        /// <summary>
        /// Default training(learning)/ predicting folder paths are passed on to the constructor.
        /// </summary>
        /// <param name="trainingFolderPath">The path to the folder containing the CSV files for training(learning).</param>
        /// <param name="predictingFolderPath">The path to the folder containing the CSV files for predicting.</param>
        public HTMAnomalyTesting(string trainingFolderPath = "training", string predictingFolderPath = "predicting")
        {
            // Folder directory set to location of C# files. This is the relative path. Use for windows.
            // string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            // _trainingFolderPath = Path.Combine(projectbaseDirectory, trainingFolderPath);
            // _predictingFolderPath = Path.Combine(projectbaseDirectory, predictingFolderPath);

            // Use the bottom path variables if you want to override to specify path on your own. Use for linux.
            _defaultTrainingFolderPath = "/workspaces/finaltest/training";
            _defaultPredictingFolderPath = "/workspaces/finaltest/predicting";

        }

        /// <summary>
        /// Runs the anomaly detection experiment with default tolerance level set to 0.1.
        /// The input parameters can be overridden.
        /// <param name="tolerance">The tolerance level for anomaly detection.</param>
        /// <param name="trainingFolderPath">The path to the folder containing the CSV files for training (learning).</param>
        /// <param name="predictingFolderPath">The path to the folder containing the CSV files for predicting.</param>
        public void RunExperiment(double tolerance = 0.1, string trainingFolderPath = null, string predictingFolderPath = null)
        {
            string actualTrainingFolderPath = trainingFolderPath ?? _defaultTrainingFolderPath;
            string actualPredictingFolderPath = predictingFolderPath ?? _defaultPredictingFolderPath;

            // HTM model training initiated
            HTMModeltraining myModel = new HTMModeltraining();
            Predictor myPredictor;

            myModel.RunHTMModelLearning(actualTrainingFolderPath, actualPredictingFolderPath, out myPredictor);

            Console.WriteLine();
            Console.WriteLine("Started testing our trained HTM Engine...................");
            Console.WriteLine();

            // Starting to test our trained HTM model
            // CSVFileReader can also be used in place of CSVFolderReader to read a single file
            // We will take sequences from predicting folder
            // After that, we will then trim those sequences: sequences where first few elements are removed, for anomaly detection
            CSVFolderReader testseq = new CSVFolderReader(actualPredictingFolderPath);
            var inputtestseq = testseq.ReadFolder();
            var triminputtestseq = CSVFolderReader.TrimSequences(inputtestseq);
            myPredictor.Reset();

            // Writing output of our experiment in csv file
            // It can later be used for visualization
            // Bottom few lines are used for setting the output path and output file name
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputFile = $"anomaly_output_{timestamp}.txt";

            // For Windows
            // string projectbaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            // string outputFolderPath = Path.Combine(projectbaseDirectory, "output");
            // string outputFilePath = Path.Combine(outputFolderPath, outputFile);

            // For Linux and Github codespaces, add the path explicitly
            // string outputFolderPath = "";
            // string outputFilePath = Path.Combine(outputFolderPath, outputFile);


            // Output path set for cloud experiment
            string outputFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            string outputFilePath = Path.Combine(outputFolderPath, outputFile);
            StoredOutputValues.OutputPath = outputFilePath;

            // Testing the sequences one by one
            // Our anomaly detection experiment is complete after all the lists are traversed iteratively.
            // If the list contains less than two values, or contain non-negative values, exception is thrown from DetectAnomaly method.
            // Errors are handled using exception handling without disrupting our program flow.
            // Used for writing our output to text file.
            // We will store the returned output of DetectAnomaly method to list of list of strings
            // Then we will use that for writing our experiment output to txt file

            List<List<String>> totalOutputStringList = new List<List<String>>();

            foreach (List<double> list in triminputtestseq)
            {
                double[] lst = list.ToArray();
                List<string> outputReturnedLines = new List<string>();

                try
                {
                    outputReturnedLines = DetectAnomaly(myPredictor, lst, tolerance);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Exception caught: {ex.Message}");
                }
                totalOutputStringList.Add(outputReturnedLines);
            }

            // Initialize a StringBuilder to efficiently build a multi-line string
            StringBuilder myStringBuilder = new StringBuilder();

            // Iterate over each inner list of strings
            foreach (List<string> innerLine in totalOutputStringList)
            {
                foreach (string line in innerLine)
                {
                    // Append the current line to the StringBuilder
                    myStringBuilder.AppendLine(line);
                }
            }

            // Write the content of the StringBuilder to output text file
            File.WriteAllText(outputFilePath, myStringBuilder.ToString());

            // Storing average accuracy of our HTM engine from cloud experiment.
            StoredOutputValues.totalAvgAccuracy = totalAccuracy / iterationCount;

            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Writing experiment results to text file successful!");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            Console.WriteLine("Anomaly detection experiment complete!!.");
            Console.WriteLine();
            Console.WriteLine("------------------------------");
        }

        /// <summary>
        /// Detects anomalies in the input list using the HTM trained model.
        /// The anomaly score is calculated using a sliding window approach.
        /// The difference between the predicted value and the actual value is used to calculate the anomaly score.
        /// If the difference exceeds a certain tolerance set earlier, anomaly is detected.
        /// Returns the result in a list of strings
        /// </summary>
        /// <param name="predictor">Trained HTM model, used for prediction.</param>
        /// <param name="list">Input list which will be used to detect anomalies.</param>
        /// <param name="tolerance">Tolerance value ratio can be overloaded from outside. Default is 0.1</param>
        private static List<string> DetectAnomaly(Predictor predictor, double[] list, double tolerance = 0.1)
        {
            // Checking if the list contains at least two values
            if (list.Length < 2)
            {
                throw new ArgumentException($"List must contain at least two values. Actual count: {list.Length}. List: [{string.Join(",", list)}]");

            }
            // Checking if the list contains any non-numeric values
            foreach (double value in list)
            {
                if (double.IsNaN(value))
                {
                    throw new ArgumentException($"List contains non-numeric values. List: [{string.Join(",", list)}]");
                }
            }

            //Storing our output to list of strings
            List<string> resultOutputStringList = new List<string>
            {
                "------------------------------",
                "",
                "Testing the sequence for anomaly detection: " + string.Join(", ", list) + ".",
                ""
            };

            // Initializing variable for calculating accuracy of each tested sequence
            double currentAccuracy = 0.0;

            // Input list will be traversed one by one, like a sliding window
            for (int i = 0; i < list.Length; i++)
            {

                //Values for the rest of the list will be iteratively referred to, in the following variable.
                var item = list[i];

                // Using our trained HTM model predictor to predict next item.
                var res = predictor.Predict(item);

                resultOutputStringList.Add("Current element in the testing sequence from input list: " + item);

                if (res.Count > 0)
                {
                    // Extracting predicted item and accuracy from predictor output
                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    var tokens3 = res.First().Similarity;

                    // We exclude the last element of the list
                    // Because there is no element after that to detect anomaly in.
                    if (i < list.Length - 1)
                    {
                        int nextIndex = i + 1;
                        double nextItem = list[nextIndex];
                        double predictedNextItem = double.Parse(tokens2.Last());

                        // Anomalyscore variable will be used to check the deviation from predicted item
                        var AnomalyScore = Math.Abs(predictedNextItem - nextItem);
                        var deviation = AnomalyScore / nextItem;

                        if (deviation <= tolerance)
                        {

                            resultOutputStringList.Add("Anomaly not detected in the next element!! HTM Engine found similarity to be: " + tokens3 + "%.");
                            currentAccuracy += tokens3;

                        }
                        else
                        {
                            resultOutputStringList.Add($"****Anomaly detected**** in the next element. HTM Engine predicted it to be {predictedNextItem} with similarity: {tokens3}%, but the actual value is {nextItem}.");
                            i++; // skip to the next element for checking, as we cannot use anomalous element for prediction
                            resultOutputStringList.Add("As anomaly was detected, so we are skipping to the next element in our testing sequence.");
                            currentAccuracy += tokens3;
                        }

                    }

                    else
                    {
                        resultOutputStringList.Add("End of input list. Further anomaly testing cannot be continued.");
                    }
                }
                else
                {
                    resultOutputStringList.Add("Nothing predicted from HTM Engine. Anomaly cannot be detected.");
                }

            }

            //Calculating HTM engine's average accuracy per sequence
            var avgSeqAccuracy = currentAccuracy / list.Length;

            resultOutputStringList.Add("");
            resultOutputStringList.Add("Average accuracy for this sequence: " + avgSeqAccuracy + "%.");
            resultOutputStringList.Add("");
            resultOutputStringList.Add("------------------------------");

            // Incrementing our HTM engine's total average accuracy outside of this method after each loop
            totalAccuracy += avgSeqAccuracy;

            //Count the number of iterations
            iterationCount++;

            return resultOutputStringList;

        }

    }
}
