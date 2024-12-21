using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using MyCloudProject.Common;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace MyExperiment
{
    public class AzureStorageProvider : IStorageProvider
    {
        private MyConfig config;

        // Constructor to initialize the AzureStorageProvider class with configuration settings.
        public AzureStorageProvider(IConfigurationSection configSection)
        {
            config = new MyConfig();
            configSection.Bind(config);
        }

        // Method to download combined training and predicting folders from Azure Blob Storage for our cloud experiment.
        public async Task<string[]> DownloadCombinedFolders(string fileName)
        {
            // Create a BlobContainerClient to interact with Azure Blob Storage.
            BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, config.TrainingContainer);

            try
            {
                // Attempt to create the container if it does not exist.
                await container.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                // If an exception occurs during container creation, wrap it with a custom message and rethrow.
                throw new Exception("Error: " + ex.Message, ex);
            }

            // Get the base path of the application.
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Define paths for the downloaded zip file and the extracted folders.
            string downloadedFilePath = Path.Combine(basePath, fileName);
            string extractedTrainingFolderPath = Path.Combine(basePath, "training");
            string extractedPredictingFolderPath = Path.Combine(basePath, "predicting");

            // Get a reference to a specific blob in the container.
            BlobClient blob = container.GetBlobClient(fileName);

            try
            {
                // Check if the blob exists in the container.
                if (await blob.ExistsAsync())
                {
                    // Download the blob to the specified file path.
                    await blob.DownloadToAsync(downloadedFilePath);

                    // Extract the downloaded zip file to the application's base path.
                    ZipFile.ExtractToDirectory(downloadedFilePath, basePath, true);

                    // Return the paths to the extracted training and predicting folders as an array.
                    return new string[] { extractedTrainingFolderPath, extractedPredictingFolderPath };
                }
                else
                {
                    // If the blob does not exist, throw a FileNotFoundException.
                    throw new FileNotFoundException();
                }
            }
            catch (Exception ex)
            {
                // Wrap and rethrow any exceptions with a custom error message.
                throw new Exception("Error: " + ex.Message, ex);
            }
        }

        // Method to upload an experiment result to Azure Table Storage.
        public async Task UploadExperimentResult(ExperimentResult result)
        {
            // Create a TableClient to interact with Azure Table Storage.
            var client = new TableClient(this.config.StorageConnectionString, this.config.ResultTable);

            // Attempt to create the table if it does not exist.
            await client.CreateIfNotExistsAsync();

            try
            {
                // Upsert the ExperimentResult entity into the table.
                await client.UpsertEntityAsync(result);

                Console.WriteLine("Uploaded to Table Storage successfully");
            }
            catch (Exception ex)
            {
                // Wrap and rethrow any exceptions with a custom error message.
                throw new Exception("Error: " + ex.Message, ex);
            }

        }

        // Method to upload a result file to Azure Blob Storage.
        public async Task UploadResultFile(string outputFileName)
        {
            // Get the connection string and file name from the configuration.
            string connectionString = this.config.StorageConnectionString;
            string csvfileName = System.IO.Path.GetFileName(outputFileName);

            // Create a BlobContainerClient to interact with Azure Blob Storage.
            BlobContainerClient container = new BlobContainerClient(connectionString, this.config.ResultContainer);

            // Ensure that the container exists; create it if it doesn't.
            await container.CreateIfNotExistsAsync();

            try
            {
                // Create a BlobClient to interact with a specific blob(file) in the container.
                BlobClient blobClient = container.GetBlobClient(csvfileName);

                // Upload the output file to Azure Blob Storage.
                await blobClient.UploadAsync(outputFileName, true);

                Console.WriteLine("Upload complete");
            }
            catch (Exception ex)
            {
                // Wrap and rethrow any exceptions with a custom error message.
                throw new Exception("Error: " + ex.Message, ex);
            }

        }

    }

}
