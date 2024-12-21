using Azure;
using Azure.Data.Tables;
using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudProject.Common
{

    public class ExperimentResult : ITableEntity, IExperimentResult
    {
        public ExperimentResult(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string ExperimentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string RequestedBy { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public double DurationSec { get; set; }

        public string TrainingFolderUrl { get; set; }

        public string PredictingFolderUrl { get; set; }

        public string OutputFileUrl { get; set; }

        public double ToleranceValue { get; set; }

        public double AvgAccuracy { get; set; }

}
}
