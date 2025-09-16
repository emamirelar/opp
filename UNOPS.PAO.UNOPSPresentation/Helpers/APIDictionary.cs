namespace UNOPS.PAO.UNOPSPresentation.Helpers;
public class APIDictionary
{
    public const string APIPrefix = "/api/";
    public const string opsAPIPrefix = "/api/unops/";

    // Project
    public const string Project = APIPrefix + "project";

    // Document
    public const string Document = APIPrefix + "document";
    public const string DocumentUpload = Document + "/upload";
    public const string DocumentLink = Document + "/link";
    public const string DocumentGenerate = Document + "/generate-document";
}