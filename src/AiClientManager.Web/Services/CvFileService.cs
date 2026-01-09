using AiClientManager.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace AiClientManager.Web.Services;

public sealed class CvFileService
{
    private readonly MongoContext _mongo;

    public CvFileService(MongoContext mongo)
    {
        _mongo = mongo;
    }

    public async Task<CvFile?> SaveCvAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return null;

        using var stream = file.OpenReadStream();
        var options = new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "contentType", file.ContentType },
                { "originalName", file.FileName }
            }
        };

        var id = await _mongo.CvBucket.UploadFromStreamAsync(file.FileName, stream, options, ct);

        string? extracted = null;
        try
        {
            extracted = await ExtractTextAsync(file, ct);
        }
        catch
        {
            extracted = null;
        }

        return new CvFile
        {
            FileId = id.ToString(),
            FileName = file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            Size = file.Length,
            ExtractedText = extracted
        };
    }

    private static async Task<string?> ExtractTextAsync(IFormFile file, CancellationToken ct)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (ext == ".txt")
        {
            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync(ct);
        }

        if (ext == ".docx")
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;
            using var doc = WordprocessingDocument.Open(ms, false);
            return doc.MainDocumentPart?.Document?.Body?.InnerText;
        }

        if (ext == ".pdf")
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;
            using var pdf = PdfDocument.Open(ms);
            var sb = new System.Text.StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(ContentOrderTextExtractor.GetText(page));
            }
            return sb.ToString();
        }

        return null;
    }
}
