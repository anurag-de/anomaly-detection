using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MyCloudProject.Common
{
    /// <summary>
    /// Defines the contract for all storage operations.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Uploadds the result of the experiment in the cloud or any other kind of store or database.
        /// </summary>
        /// <param name="outputFileName">The name of the file at some remote (cloud) location  where the file will be uploaded.</param>
        /// <returns>Not used. It can be null.</returns>
        Task UploadResultFile(string outputFileName);

        /// <summary>
        /// Uploads results of the experiment to the remote (cloud) location. For example the fileshare or as the entity to the table storage.
        /// </summary>
        /// <param name="result"></param>
        /// <returns>Not used.</returns>
        Task UploadExperimentResult(ExperimentResult result);

        /// <summary>
        /// Downloads a combined set of folders specified by the given file name from a remote (cloud) location.
        /// </summary>
        /// <param name="fileName">The name of the zip file that represents the combined folders.</param>
        /// <returns>An array of strings representing the contents of the combined folders.</returns>
        Task<string[]> DownloadCombinedFolders(string fileName);
    }
}
