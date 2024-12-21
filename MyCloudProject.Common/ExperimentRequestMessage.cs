using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudProject.Common
{
    public class ExperimentRequestMessage : IExperimentRequestMessage
    {
        public string ExperimentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CombinedFolder { get; set; }
        public string RequestedBy { get; set; }
        public double ToleranceValue { get; set; }
    }
}


/*

Sample experiment request message
 
{
    "ExperimentId": "a001",
    "Name": "ML22/23-12",    
    "Description": "Implement Anomaly Detection Sample",
    "CombinedFolder": "dataset_small.zip", 
    "RequestedBy": "Anurag",
    "ToleranceValue": 0.1    
}

 
 */ 