using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WaterSupplySimulator.Data;
using WaterSupplySimulator.DTOs;
using WaterSupplySimulator.Models;

namespace WaterSupplySimulator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WaterSystemController : ControllerBase
{
    private readonly IRepository<SensorReading> _sensorRepo;
    private readonly IRepository<PumpState> _pumpRepo;
    private readonly IRepository<Alert> _alertRepo;
    private readonly IRepository<EventLog> _logRepo;
    private readonly IMapper _mapper;

    public WaterSystemController(
        IRepository<SensorReading> sensorRepo,
        IRepository<PumpState> pumpRepo,
        IRepository<Alert> alertRepo,
        IRepository<EventLog> logRepo,
        IMapper mapper)
    {
        _sensorRepo = sensorRepo;
        _pumpRepo = pumpRepo;
        _alertRepo = alertRepo;
        _logRepo = logRepo;
        _mapper = mapper;
    }

    [HttpGet("sensors")]
    public async Task<ActionResult<IEnumerable<SensorReadingDto>>> GetSensorReadings()
    {
        var readings = (await _sensorRepo.GetAllAsync())
            .OrderByDescending(r => r.Timestamp)
            .Take(100)
            .ToList();
        return Ok(_mapper.Map<IEnumerable<SensorReadingDto>>(readings));
    }

    [HttpGet("pump")]
    public async Task<ActionResult<PumpStateDto>> GetPumpState()
    {
        var pump = (await _pumpRepo.GetAllAsync()).FirstOrDefault() ?? new PumpState { IsOn = false, Mode = "Auto" };
        return Ok(_mapper.Map<PumpStateDto>(pump));
    }

    [HttpPost("pump/manual-toggle")]
    [Authorize]
    public async Task<ActionResult> TogglePump()
    {
        var pump = (await _pumpRepo.GetAllAsync()).First();
        pump.Mode = "Manual";
        pump.IsOn = !pump.IsOn;
        pump.LastChanged = DateTime.UtcNow;

        if (!TryValidateModel(pump))
        {
            return BadRequest(ModelState);
        }

        await _logRepo.AddAsync(new EventLog
        {
            EventType = "Pump",
            Message = $"Pump manually turned {(pump.IsOn ? "ON" : "OFF")}",
            Timestamp = DateTime.UtcNow
        });
        await _pumpRepo.SaveChangesAsync();
        await _logRepo.SaveChangesAsync();
        return Ok(_mapper.Map<PumpStateDto>(pump));
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlerts()
    {
        var alerts = (await _alertRepo.GetAllAsync()).Where(a => !a.IsAcknowledged).ToList();
        return Ok(_mapper.Map<IEnumerable<AlertDto>>(alerts));
    }

    [HttpPost("alerts/{id}/acknowledge")]
    [Authorize]
    public async Task<ActionResult> AcknowledgeAlert(int id)
    {
        var alert = await _alertRepo.GetByIdAsync(id);
        if (alert == null) return NotFound();
        alert.IsAcknowledged = true;
        await _alertRepo.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("logs")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EventLogDto>>> GetLogs()
    {
        var logs = (await _logRepo.GetAllAsync()).OrderByDescending(l => l.Timestamp).Take(100).ToList();
        return Ok(_mapper.Map<IEnumerable<EventLogDto>>(logs));
    }

    [HttpGet("logs/csv")]
    [Authorize]
    public async Task<IActionResult> DownloadLogsCsv()
    {
        var logs = (await _logRepo.GetAllAsync()).OrderByDescending(l => l.Timestamp).ToList();
        var csv = "Id,EventType,Message,Timestamp\n" +
                  string.Join("\n", logs.Select(l => $"{l.Id},{l.EventType},\"{l.Message}\",{l.Timestamp:yyyy-MM-dd HH:mm:ss}"));
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "event_logs.csv");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Password != "SuperSecretKey12345")
            return Unauthorized();

        var jwtKey = "SuperSecretKey12345_ChangeMe_1234567890";
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new List<Claim>(),
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    public class LoginRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}