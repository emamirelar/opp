namespace UNOPS.PAO.Domain.Enums
{
    /// <summary>
    /// Defines the status of a risk in the risk register
    /// </summary>
    public enum RiskStatus
    {
        /// <summary>
        /// Risk is open and requires attention
        /// </summary>
        Open = 1,

        /// <summary>
        /// Risk is under review
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// Risk has been mitigated
        /// </summary>
        Mitigated = 3,

        /// <summary>
        /// Risk has been accepted
        /// </summary>
        Accepted = 4,

        /// <summary>
        /// Risk has been closed
        /// </summary>
        Closed = 5
    }
}

