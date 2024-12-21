namespace MyExperiment
{
    /// <summary>
    /// This class is using for storing runtime metrics for our cloud experiment.
    /// </summary>
    public static class StoredOutputValues
    {
        public static double TrainingTimeInSeconds { get; set; }
        public static string OutputPath { get; set; }
        public static double totalAvgAccuracy { get; set; }
    }
}
