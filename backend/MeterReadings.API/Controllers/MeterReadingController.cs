using MeterReadings.Core.Interfaces.Services;
using MeterReadings.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.API.Controllers;

[ApiController]
[Route("api")]
public class MeterReadingController : ControllerBase
{
    private readonly IMeterReadingService _meterReadingService;
    private readonly ILogger<MeterReadingController> _logger;
    private readonly MeterReadingDbContext _dbContext;

    public MeterReadingController(
        IMeterReadingService meterReadingService,
        MeterReadingDbContext dbContext,
        ILogger<MeterReadingController> logger)
    {
        _meterReadingService = meterReadingService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("db-status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();

            return Ok(new
            {
                CanConnect = canConnect,
                PendingMigrations = pendingMigrations.ToList(),
                AppliedMigrations = appliedMigrations.ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking database status");
            return StatusCode(500, new { Error = ex.Message });
        }
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