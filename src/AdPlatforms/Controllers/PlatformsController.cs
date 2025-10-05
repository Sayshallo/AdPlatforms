using AdPlatforms.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatforms.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformService _service;
    private readonly ILogger<PlatformsController> _logger;

    public PlatformsController(IPlatformService service, ILogger<PlatformsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // Загрузка файла (multipart/form-data, поле "file")
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null) return BadRequest("No file uploaded (form field 'file').");
        try
        {
            using var stream = file.OpenReadStream();
            var res = await _service.LoadFromStreamAsync(stream);
            return Ok(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed");
            return StatusCode(500, "Internal server error");
        }
    }

    // Альтернативно: загрузка raw text в теле (text/plain)
    [HttpPost("upload/text")]
    [Consumes("text/plain")]
    public async Task<IActionResult> UploadText([FromBody] string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return BadRequest("Empty content");
        try
        {
            using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var res = await _service.LoadFromStreamAsync(ms);
            return Ok(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload text failed");
            return StatusCode(500, "Internal server error");
        }
    }

    // Список всех площадок (для отладки)
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_service.GetAllPlatforms());
    }

    // Поиск по локации (query param ?location=/ru/svrd/revda)
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return BadRequest("Query param 'location' required.");
        var result = _service.FindPlatformsForLocation(location);
        return Ok(result);
    }
}
