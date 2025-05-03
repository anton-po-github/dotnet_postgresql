using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AzureStorageController : Controller
{
    private readonly IBlobService _blobService;
    private readonly string _connectionString = "";
    private readonly string _container = "";
    public AzureStorageController(IBlobService blobService, IConfiguration config)
    {
        _blobService = blobService;

        // _connectionString = config["AzureStorageConfig:StorageConnection"];

        //   _container = config["AzureStorageConfig:ContainerName"];
    }

    [HttpGet("ListFiles")]
    public async Task<List<string>> ListFiles()
    {
        return await _blobService.GetAllDocuments(_connectionString, _container);
    }

    [Route("InsertFile")]
    [HttpPost]
    public async Task<bool> InsertFile([FromForm] IFormFile asset)
    {
        if (asset != null)
        {
            Stream stream = asset.OpenReadStream();

            await _blobService.UploadDocument(_connectionString, _container, asset.FileName, stream);

            return true;
        }

        return false;
    }

    [HttpGet("DownloadFile/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        var content = await _blobService.GetDocument(_connectionString, _container, fileName);

        return File(content, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
    }

    [Route("DeleteFile/{fileName}")]
    [HttpGet]
    public async Task<bool> DeleteFile(string fileName)
    {
        return await _blobService.DeleteDocument(_connectionString, _container, fileName);
    }
}
