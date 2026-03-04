namespace UNOPS.PAO.UNOPSBusiness.Models;

using System;
using System.Collections.Generic;

public class MyPubSubMessage
{
    public string MessageType { get; set; } = string.Empty; // "EntityProcessing" or "BulkImport"
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }

    public string? PromptType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? BatchData { get; set; }
    public int UserId { get; set; } // User who initiated the bulk import
    public string? FileId { get; set; } // Google Sheet ID for bulk import operations
}