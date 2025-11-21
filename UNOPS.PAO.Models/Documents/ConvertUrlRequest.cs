namespace UNOPS.PAO.Models.Documents;

public class ConvertUrlRequest
{
    public bool IncludeJson { get; set; } = false;
    public string OutputFormat { get; set; } = "markdown";
    public string GcsOutput { get; set; } = "";
    public int ChunkSize { get; set; } = 1;
    public string EmbeddingsModel { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
}

