namespace UNOPS.PAO.Domain.Enums
{
    /// <summary>
    /// Defines the impact levels for risks
    /// NOTE: This enum is deprecated. Use RiskImpactLevel entity instead for oUP alignment.
    /// Kept for backward compatibility with existing code.
    /// </summary>
    public enum RiskImpact
    {
        /// <summary>
        /// Low impact risk
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium impact risk
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High impact risk
        /// </summary>
        High = 3
    }
}

