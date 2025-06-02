using Microsoft.EntityFrameworkCore;

namespace WaterSupplySimulator.Data;

public class WaterSystemContext : DbContext
{
    public WaterSystemContext(DbContextOptions<WaterSystemContext> options)
        : base(options) { }

    public DbSet<WaterSupplySimulator.Models.SensorReading> SensorReadings { get; set; } = null!;
    public DbSet<WaterSupplySimulator.Models.PumpState> PumpStates { get; set; } = null!;
    public DbSet<WaterSupplySimulator.Models.Alert> Alerts { get; set; } = null!;
    public DbSet<WaterSupplySimulator.Models.EventLog> EventLogs { get; set; } = null!;
    public DbSet<WaterSupplySimulator.Models.ControlSettings> ControlSettings { get; set; } = null!;
}