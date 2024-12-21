using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyCloudProject.Common
{
    public interface IExperiment
    {
        /// <summary>
        /// Runs the experiment with the specified parameters.
        /// </summary>
        /// <param name="inputTrainingFolder">The path to the training folder.</param>
        /// <param name="inputPredictingFolder">The path to the predicting folder.</param>
        /// <param name="expID">The experiment ID.</param>
        /// <param name="rBy">The user who requested the experiment.</param>
        /// <param name="nme">The name of the experiment.</param>
        /// <param name="dscpn">The description of the experiment.</param>
        /// <param name="tValue">The tolerance value for anomaly detection.</param>
        /// <returns>Task representing the asynchronous operation, returning the ExperimentResult.</returns>
        Task<ExperimentResult> Run(string inputTrainingFolder, string inputPredictingFolder, string expID, string rBy, string nme, string dscpn, double tValue);

        /// <summary>
        /// Starts the listening for incomming messages, which will trigger the experiment.
        /// </summary>
        /// <param name="cancelToken">Token used to cancel the listening process.</param>
        /// <returns></returns>
        Task RunQueueListener(CancellationToken cancelToken);
    }
}
