using System.Text.RegularExpressions;

namespace UNOPS.PAO.UNOPSBusiness.Helpers;

public static class GoogleDriveHelper
{
    public static string GetFileIdFromDriveLink(this string driveLink)
    {
        if (string.IsNullOrWhiteSpace(driveLink))
        {
            throw new ArgumentException("The drive link cannot be null or empty.", nameof(driveLink));
        }

        var start = driveLink.IndexOf("d/") + 2;
        // excel files contain edit and not view in the link like pdfs
        var end = driveLink.IndexOf("/view") != -1 ? driveLink.IndexOf("/view") : driveLink.IndexOf("/edit");
        if (end < 0)
        {
            start = driveLink.IndexOf("id=") + 3;
            end = driveLink.Length;
        }

        return driveLink.Substring(start, end - start);
    }
}