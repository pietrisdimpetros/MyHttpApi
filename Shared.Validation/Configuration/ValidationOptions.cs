namespace Shared.Validation.Configuration
{
    public sealed class ValidationOptions
    {
        public const string SectionName = "Validation";

        /// <summary>
        /// If true, the validator stops checking after the first error.
        /// Useful for performance in high-load scenarios.
        /// </summary>
        public bool EnableFailFast { get; set; } = false;
    }
}