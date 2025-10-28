using System.Collections.Generic;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.OrganizationUnits
{
    /// <summary>
    /// Response model for organization unit recent updates including the org unit name
    /// </summary>
    public class OrgUnitRecentUpdatesResponse
    {
        /// <summary>
        /// List of recent updates for the organization unit
        /// </summary>
        public List<RecentUpdateModel> Updates { get; set; } = new List<RecentUpdateModel>();

        /// <summary>
        /// Name of the organization unit that the updates belong to
        /// </summary>
        public string OrgUnitName { get; set; } = "your organization unit";

        /// <summary>
        /// ID of the organization unit that the updates belong to
        /// </summary>
        public int? OrgUnitId { get; set; }
    }
}
