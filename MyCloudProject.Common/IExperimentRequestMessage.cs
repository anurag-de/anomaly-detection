using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudProject.Common
{
    /// <summary>
    /// Defines the contract for the message request that will run your experiment.
    /// </summary>
    public interface IExperimentRequestMessage
    {
        string ExperimentId { get; set; }
        string Name { get; set; }
        string CombinedFolder { get; set; }
        string RequestedBy { get; set; }
        string Description { get; set; }
        double ToleranceValue { get; set; }

    }
}
