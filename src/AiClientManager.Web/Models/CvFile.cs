namespace AiClientManager.Web.Models;

public sealed class CvFile
{
    public string FileId { get; set; } = string.Empty; // GridFS ObjectId string
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long Size { get; set; }
    public string? ExtractedText { get; set; }
}
