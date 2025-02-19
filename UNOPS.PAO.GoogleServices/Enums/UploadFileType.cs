namespace UNOPS.PAO.GoogleServices.Enums;

using System.ComponentModel;

public enum UploadFileType
{
    [Description("Invoice")] Invoice,
    [Description("Claim")] Claim,
    [Description("Grant")] Grant,
    [Description("Contract")] Contract
}
