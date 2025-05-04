using MongoDB.Bson;
using MongoDB.Driver.GridFS;

public class FileService
{
    private readonly MongoDBContext _mongoDBContext;

    public FileService(MongoDBSettings databaseSettings)
    {
        _mongoDBContext = new MongoDBContext(databaseSettings);
    }

    public async Task<ObjectId> UploadBytesAsync(string fileName, byte[] data)
    {
        try
        {
            using var ms = new MemoryStream(data);
            return await _mongoDBContext.Bucket.UploadFromStreamAsync(fileName, ms);
        }
        catch (Exception ex)
        {
            // логирование
            throw new AppException($"Error uploading file: {ex.Message}");
        }
    }

    public async Task DeleteFileAsync(ObjectId id)
    {
        try
        {
            await _mongoDBContext.Bucket.DeleteAsync(id);
        }
        catch (GridFSFileNotFoundException)
        {
            // можно логировать, но игнорировать, если файла уже нет
        }
        catch (Exception ex)
        {
            throw new AppException($"Error deleting file: {ex.Message}");
        }
    }

    public void DeleteFile(string id)
    {
        var objectId = ObjectId.Parse(id);

        _mongoDBContext.Bucket.DeleteAsync(objectId);
    }
}

