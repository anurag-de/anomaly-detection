using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyExperiment
{
    /// <summary>
    /// This class implements the ML experiment that will run in the cloud. This is the refactored code from my SE project.
    /// </summary>
    public class Experiment : IExperiment
    {
        private IStorageProvider storageProvider;

        private ILogger logger;

        private MyConfig config;

        public Experiment(IConfigurationSection configSection, IStorageProvider storageProvider, ILogger log)
        {
            this.storageProvider = storageProvider;
            this.logger = log;
            config = new MyConfig();
            configSection.Bind(config);
        }
        public Task<ExperimentResult> Run(string inputTrainingFolder, string inputPredictingFolder, string expID, string rBy, string nme, string dscpn, double tValue)
        {
            string rowKey = Guid.NewGuid().ToString();

            HTMAnomalyTesting anomalydetect = new HTMAnomalyTesting();

            ExperimentResult res = new ExperimentResult(expID, rowKey);

            // Setting properties of our ExperimentResult object with relevant information
            res.StartTimeUtc = DateTime.UtcNow;
            anomalydetect.RunExperiment(tValue, inputTrainingFolder, inputPredictingFolder);
            res.EndTimeUtc = DateTime.UtcNow;
            res.TrainingFolderUrl = inputTrainingFolder;
            res.PredictingFolderUrl = inputPredictingFolder;
            res.DurationSec = StoredOutputValues.TrainingTimeInSeconds;
            res.ExperimentId = "ex-" + expID;
            res.RequestedBy = rBy;
            res.Name = nme;
            res.Description = dscpn;
            res.ToleranceValue = tValue;
            res.Timestamp = DateTime.UnixEpoch;
            res.OutputFileUrl = StoredOutputValues.OutputPath;
            res.AvgAccuracy = StoredOutputValues.totalAvgAccuracy;

            return Task.FromResult<ExperimentResult>(res);
        }
        public async Task RunQueueListener(CancellationToken cancelToken)
        {
            // Create a QueueClient to interact with Azure Storage Queue
            QueueClient queueClient = new QueueClient(this.config.StorageConnectionString, this.config.Queue);

            // Continuously listen to the queue until cancellation is requested
            while (cancelToken.IsCancellationRequested == false)
            {
                // Receive a message from the queue asynchronously
                QueueMessage message = await queueClient.ReceiveMessageAsync();

                if (message != null)
                {
                    try
                    {
                        // Extract the message text from the received message
                        string msgTxt = Encoding.UTF8.GetString(message.Body.ToArray());

                        this.logger?.LogInformation($"{DateTime.Now} - Received the message {msgTxt}");

                        // Deserialize the received JSON message into an ExperimentRequestMessage object
                        ExperimentRequestMessage request = JsonSerializer.Deserialize<ExperimentRequestMessage>(msgTxt);

                        // Download the training and predicting folders specified in the request message
                        string[] extractedFolderPaths = await this.storageProvider.DownloadCombinedFolders(request.CombinedFolder);

                        string inputTrainingFolder = extractedFolderPaths[0];
                        string inputPredictingFolder = extractedFolderPaths[1];

                        this.logger?.LogInformation($"{DateTime.Now} - Downloaded the training folder: {inputTrainingFolder}");

                        this.logger?.LogInformation($"{DateTime.Now} - Downloaded the predicting folder: {inputPredictingFolder}");

                        // Run the experiment using the extracted folder paths and request message details
                        ExperimentResult result = await this.Run(inputTrainingFolder, inputPredictingFolder, request.ExperimentId, request.RequestedBy, request.Name, request.Description, request.ToleranceValue);

                        // Upload the experiment's output file to storage
                        await storageProvider.UploadResultFile(result.OutputFileUrl);

                        this.logger?.LogInformation($"{DateTime.Now} - Finished uploading csv output file.");

                        // Upload the experiment result to results table
                        await storageProvider.UploadExperimentResult(result);

                        this.logger?.LogInformation($"{DateTime.Now} - Finished uploading results to table storage. Experiment completed successfully.");

                        // Delete the processed message from the queue
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                    }
                    catch (Exception ex)
                    {
                        this.logger?.LogError(ex, "TODO...");
                    }
                }
                else
                {
                    await Task.Delay(500);
                    logger?.LogTrace("Queue empty...");
                }
            }

            this.logger?.LogInformation("Cancel pressed. Exiting the listener loop.");
        }

    }
}
