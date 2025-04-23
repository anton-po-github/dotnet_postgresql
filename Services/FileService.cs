using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

public class FileService
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly MongoDBContext _mongoDBContext = null;

    public FileService(IWebHostEnvironment hostEnvironment, MongoDBSettings databaseSettings)
    {
        _hostEnvironment = hostEnvironment;
        _mongoDBContext = new MongoDBContext(databaseSettings);
    }

    public async Task<ObjectId> UploadFile(IFormFile file)
    {
        try
        {
            var stream = file.OpenReadStream();
            var filename = file.FileName;
            return await _mongoDBContext.Bucket.UploadFromStreamAsync(filename, stream);
        }
        catch (Exception ex)
        {
            // log or manage the exception
            return new ObjectId(ex.ToString());
        }
    }

    public Task<byte[]> GetBytesByName(string fileName)
    {
        return _mongoDBContext.Bucket.DownloadAsBytesByNameAsync(fileName);
    }

    public void DeleteFile(string id)
    {
        var objectId = MongoDB.Bson.ObjectId.Parse(id);
        _mongoDBContext.Bucket.DeleteAsync(objectId);
    }

    public async Task<string> Save(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName)?.ToLower();
        var fileName = Guid.NewGuid() + extension;

        var dir = Path.Combine(_hostEnvironment.WebRootPath ?? _hostEnvironment.ContentRootPath,
            "uploads");

        var absolute = Path.Combine(dir, fileName);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await using var writer = new FileStream(absolute, FileMode.CreateNew);
        await file.CopyToAsync(writer);

        return absolute;
    }

    public async Task DownloadFileMongo()
    {
        var client = new MongoClient("mongodb://localhost:27017");

        var database = client.GetDatabase("BookstoreDb");

        var gridFsBucketOptions = new GridFSBucketOptions()
        {
            BucketName = "images",
            ChunkSizeBytes = 1048576, // 1МБ
        };

        var bucket = new GridFSBucket(database, gridFsBucketOptions);

        var filter = Builders<GridFSFileInfo<ObjectId>>

            .Filter.Eq(x => x.Filename, "123.png");

        var searchResult = await bucket.FindAsync(filter);

        var fileEntry = searchResult.FirstOrDefault();

        byte[] content = await bucket.DownloadAsBytesAsync(fileEntry.Id);

        File.WriteAllBytes("123.png", content);
    }

    public byte[] CreateSpecialByteArray(int length)
    {
        var arr = new byte[length];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = 0x20;
        }
        return arr;
    }

    public async Task<String> GetFileInfo(string id)
    {
        GridFSFileInfo info = null;
        var objectId = new ObjectId(id);
        try
        {
            using (var stream = await _mongoDBContext.Bucket.OpenDownloadStreamAsync(objectId))
            {
                info = stream.FileInfo;
            }
            return info.Filename;
        }
        catch (Exception)
        {
            return "Not Found";
        }
    }
    public async Task<string> UploadProfilePicture(IFormFile file)
    {
        /*if (file == null || file.Length == 0)
            throw new UserFriendlyException("Please select profile picture");*/

        var folderName = Path.Combine("Uploads", "Images");
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        var uniqueFileName = folderName;

        var dbPath = Path.Combine(folderName, uniqueFileName);

        using (var fileStream = new FileStream(Path.Combine(filePath, uniqueFileName), FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return dbPath;
    }

}

