namespace Shared.Logging.Configuration
{
    public sealed class LoggingOptions
    {
        public const string SectionName = "Logging:Enterprise";

        /// <summary>
        /// If true, outputs structured JSON (best for Cloud/Splunk/Seq).
        /// If false, outputs readable text (best for Local Dev).
        /// </summary>
        public bool UseJsonFormat { get; set; } = false;

        /// <summary>
        /// If true, mutes the default chatty ASP.NET request logs 
        /// and uses our custom summary logger instead.
        /// </summary>
        public bool EnableCustomRequestLogging { get; set; } = true;
    }
}
