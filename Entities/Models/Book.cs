using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string BookName { get; set; } = default!;
    public decimal Price { get; set; }
    public string Category { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string IconBase64 { get; set; } = default!;
    public string IconFileName { get; set; } = default!;
    public string? IconId { get; set; }
    public string? IconPath { get; set; }
    public string? TypeFile { get; set; }
}

