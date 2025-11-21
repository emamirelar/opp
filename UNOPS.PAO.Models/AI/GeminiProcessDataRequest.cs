namespace UNOPS.PAO.Models.AI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class GeminiProcessDataRequest
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? DocumentStoragePath { get; set; }  // Google Cloud Storage URI (gs://bucket/path)
    public string? DocumentMimeType { get; set; }     // MIME type for the document
}