namespace UNOPS.PAO.UNOPSPresentation.Helpers;
public class APIDictionary
{
    public const string APIPrefix = "/api/";
    public const string opsAPIPrefix = "/api/unops/";


    // Document
    public const string Document = APIPrefix + "document";
    public const string DocumentUpload = Document + "/upload";
    public const string DocumentLink = Document + "/link";

    // Base Engagement routes (read-only)
    public const string BaseEngagements = APIPrefix + "base-engagements";
    public const string BaseEngagement = APIPrefix + "base-engagements";
    public const string BaseEngagementsByPartner = APIPrefix + "partners";
    public const string BaseEngagementPartners = APIPrefix + "base-engagements";
}