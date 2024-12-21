# Implementing anomaly detection in .NET/Azure using NeocortexAPI

Tech used: .NET/Azure

# Introduction:

HTM (Hierarchical Temporal Memory) is a machine learning algorithm with immense potential. It uses a hierarchical network of nodes to process time-series data in a distributed way. Each node, or column, can be trained to learn and recognize patterns in input data, making it a promising approach for anomaly detection and prediction in various applications. The use of the multisequencelearning class from NeoCortex API to implement the anomaly detection system, where numerical sequences are read from multiple CSV files inside a folder, further demonstrates the algorithm's versatility. After training an HTM engine, the same trained engine is used for learning patterns and detecting anomalies, showcasing the algorithm's adaptability.

The project code for the anomaly detection system has been containerized adequately so that it can be run in Azure Cloud without any issues. 

# Project overview

The diagram given below gives us an overview of how the project runs in Azure cloud.

```mermaid
flowchart LR
  subgraph Initialization
    A[2. Trigger Queue Creation] --> B(7. Submit Trigger Message)
  end

  subgraph Data Preparation
    C[3. Create Blob Container] --> D(4. Upload training/predicting Files for experiment)
  end

  subgraph Code Deployment
    E[1. Push .NET code to ACR as docker image] -->F(5. Deploy Docker Image to ACI)
    F -->G(6. Image deployed as Docker Container, awaiting trigger message)
  end

  subgraph Execution
    B --> G
    G --> H(8. Docker container starts experiment)
    D --> H
    H --> I(9. Experiment Complete)
  end

  subgraph Results
    I --> J(10. Result uploaded to Table Storage)
    I --> K(11. Result file written to Blob Storage)
  end
```
# Description of the project

N.B: The project is not live on Azure Cloud now; however, it can be re-run by following the steps below and configuring the components below in Azure Cloud.

The names of the major components used in the project in cloud is given in the table below. 

| Components | Name | Description |
| --- | --- | --- |
| [Resource Group](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal#what-is-a-resource-group) | ```CCProjectR``` | --- |
| [Container Registry](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-intro) | ```CCProjectC``` | --- |
| [Container Registry server](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-intro) | ```ccprojectc.azurecr.io``` | --- |
| [Repository](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-concepts) | ```adecloudproject:latest``` | Name of my repository |
| [Container Instance](https://learn.microsoft.com/en-us/azure/container-instances/container-instances-overview) | ```adecloudprojectcontainer``` | Name of the container instance where experiment runs |
| [Storage account](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-overview) | ```ccprojectsd``` | --- |
| [Queue storage](https://learn.microsoft.com/en-us/azure/storage/queues/storage-queues-introduction) | ```ade-triggerqueue``` | Queue where trigger message is passed to initiate experiment|
| [Blob container](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction) | ```ade-trainingfiles``` | Training and prediction folders with csv files combined as zip and uploaded |
| [Blob container](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction) | ```ade-resultfiles``` | Used to store result csv written after completion of experiment |
| [Table storage](https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-overview) | ```aderesultstable``` | Table used to store result of the experiment |

All the components were deployed in ***Germany West Central***.

Commands to pull image from:

Azure container registry
```
docker pull ccprojectc.azurecr.io/adecloudproject:latest
````
Docker Hub
```
docker pull anuragdefuas/mycloudproject:latest
````

To start the experiment and run the cloud project, we will have to pass message in JSON format, similar to the one given below, to 'ade-triggerqueue' queue:

```json
{
    "ExperimentId": "ads00", ///Add you experiment ID
    "Name": "ML22/23-12",    
    "Description": "Implement Anomaly Detection Sample",
    "CombinedFolder": "mydataset_4.zip", ///Add the correct training zipfile name
    "RequestedBy": "Anurag", ///Add your name
    "ToleranceValue": 0.1 ///Add the tolerance value ratio 
}
 ```
This message will act as trigger to start the experiment.

Brief description of this message is given in the table below:

| JSON Key | Description |
|---------------------------------------|------------|
| ExperimentId | ExperimentID string for the experiment |
| Name | Name of the experiment |
| Description | Brief description of the experiment |
| CombinedFolder | Name of the zip file stored in storage container; will be used for experiment |
| RequestedBy | Name of the requester |
| ToleranceValue | Tolerance value (ratio) for the experiment |

The zip file, which contains our training and predicting folders should be uploaded to blob storage container 'ade-trainingfiles' before the experiment is started. It will be used in our experiment.

Some training files are already kept in the container. Please note that the file **"mydataset_2.zip"** can we used for trial cloud experiment (contains a small dataset, takes around 20-25 mins). Rest of the files take a longer time for HTM training. Numbers in the name of the zip files signify the number of sequences that are used for training.

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/training_blob.png)

The name of the zip file used for the experiment should be provided in the input JSON message. The folder structure inside the zip file ***must*** be in the following format.

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/folder_structure.png)

The requestor should provide the JSON message to the 'ade-triggerqueue' queue like this (Please uncheck the box which asks to encode the message body in Base64): 

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/trigger_queue.png)

Once this is done, our experiment starts in the container instance 'adecloudprojectcontainer' (4-core, 10 GB memory), which has already been deployed in azure container instances.

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/container_instance.png)

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/running_container1%20.png)

The container instance was deployed from azure container registry, where the containerized (dockerized) project code had already been pushed earlier from my local machine.

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/repository.png)

Once our experiment is complete, it should look something like this, 

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/running_container2.png)

Th results from our experiment are written to our table 'aderesultstable':

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/result_table.png)

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/result_table_detail.png)

The text output file from our experiment is uploaded to blob storage container 'ade-resultfiles'.

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/result_blob.png)

The project code for running the experiment in cloud uses [MultiSequenceLearning](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) class in NeoCortex API for training our HTM Engine as it's base, so the algorithm remains mostly the same. The code has been refactored to run properly in the cloud.  The most significant changes made are:

* Added new class to store new experiment result metrics for writing to results table.

```csharp
    public static class StoredOutputValues
    {
        public static double TrainingTimeInSeconds { get; set; }
        public static string OutputPath { get; set; }
        public static double totalAvgAccuracy { get; set; }
    }
```

* Modified RunExperiment so that it accepts the training/ predicting folder paths, and tolerance values as parameters.

```diff
public class myExperiment
{
   public static void Main()
   {
        HTMAnomalyTesting anomalydetect = new HTMAnomalyTesting();
-       anomalydetect.RunExperiment();
+       anomalydetect.RunExperiment(tValue, inputTrainingFolder, inputPredictingFolder);
   }
}
```

* Added file writing capability. 

![image](https://github.com/ade-ghub/anomaly-detection/blob/main/MyExperiment/misc/result_txt.png)
  
The results table contain the following parameters:

| Parameter | Description |
|---------------------------------------|------------|
| PartitionKey | Defines the logical partition within a table |
| RowKey | Helps in unique identification of an entity |
| Timestamp | Timestamp of the experiment |
| Description | Small description of the experiment |
| DurationSec | Description of the experiment |
| StartTimeUtc | Start time of the experiment |
| EndTimeUtc | End time of the experiment |
| ExperimentId | Experiment ID of the experiment, given by user |
| Name | Name of the requester |
| OutputFileUrl | Path of the output CSV file |
| TrainingFolderUrl | Path of the training folder |
| PredictingFolderUrl | Path of the predicting folder |
| RequestedBy | Name of the requestor |
| ToleranceValue | Double value of the tolerance ratio |
| AvgAccuracy | Accuracy of our trained HTM engine in anomaly detection |

Brief description about some important C# methods for the cloud project:

* DownloadCombinedFolders method is used for downloading the zip file needed for the experiment to the application's base path, and then returns the paths to the extracted training and predicting folders as an array.

```csharp

public async Task<string[]> DownloadCombinedFolders(string fileName)
        {
            BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, config.TrainingContainer);
            ............
            ZipFile.ExtractToDirectory(downloadedFilePath, basePath, true);
            ............
            ............
            return new string[] { extractedTrainingFolderPath, extractedPredictingFolderPath };
            ............

        }               
   
```

* UploadExperimentResult() and UploadResultFile() methods upload the experiment result to table - 'aderesultstable' and blob container - 'ade-resultfiles' respectively.
* Run() method runs the main refactored SE code(Experiment). We then use an instance of ExperimentResult class to store our result.
* RunQueueListener() waits for a trigger message, and runs the whole experiment sequentially after trigger message is received.
