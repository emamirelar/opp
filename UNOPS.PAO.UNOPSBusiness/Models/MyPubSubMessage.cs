namespace UNOPS.PAO.UNOPSBusiness.Models;

using System;
using System.Collections.Generic;

public class MyPubSubMessage
{
    public string MessageType { get; set; } // "EntityProcessing" or "BulkImport"
    public string EntityName { get; set; }
    public int? EntityId { get; set; }

    public string? PromptType { get; set; }
    public string Content { get; set; }
    public string? BatchData { get; set; }
    public int UserId { get; set; } // User who initiated the bulk import
}