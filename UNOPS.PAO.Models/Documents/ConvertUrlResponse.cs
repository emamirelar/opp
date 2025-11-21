namespace UNOPS.PAO.Models.Documents;

public class ConvertUrlResponse
{
    public string? Status { get; set; }
    public ConvertUrlResponseData? Response { get; set; }
    public string? Error { get; set; }
}

public class ConvertUrlResponseData
{
    public string? Markdown { get; set; }
    public string? Text { get; set; }
    public string? GcsPath { get; set; }
    public ConvertUrlMetadata? Metadata { get; set; }
}

public class ConvertUrlMetadata
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public int? ChunkCount { get; set; }
}

