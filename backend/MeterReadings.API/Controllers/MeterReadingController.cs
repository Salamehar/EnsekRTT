using Microsoft.AspNetCore.Mvc;
using MeterReadings.Core.Interfaces.Services;

namespace MeterReadings.API.Controllers;

[ApiController]
[Route("api")]
public class MeterReadingController : ControllerBase
{
    private readonly IMeterReadingService _meterReadingService;
    private readonly ILogger<MeterReadingController> _logger;

    public MeterReadingController(
        IMeterReadingService meterReadingService,
        ILogger<MeterReadingController> logger)
    {
        _meterReadingService = meterReadingService;
        _logger = logger;
    }

    [HttpPost("meter-reading-uploads")]
    public async Task<IActionResult> UploadMeterReadings(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided or file is empty");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only CSV files are supported");
            }

            using var stream = file.OpenReadStream();
            var result = await _meterReadingService.ProcessMeterReadingsAsync(stream, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing meter readings");
            return StatusCode(500, "An error occurred while processing the file");
        }
    }
}