namespace UNOPS.PAO.Domain.Enums
{
    /// <summary>
    /// Defines whether an entity is currently in an approval workflow
    /// </summary>
    public enum WorkflowStatus
    {
        /// <summary>
        /// Entity is not in any approval workflow
        /// </summary>
        None = 0,

        /// <summary>
        /// Entity is currently in an approval workflow
        /// </summary>
        InWorkflow = 1
    }
}
