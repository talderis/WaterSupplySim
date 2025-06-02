using System.ComponentModel.DataAnnotations;

namespace WaterSupplySimulator.Models;

public class ControlSettings
{
    public int Id { get; set; }
    [Range(0, 1000)]
    public double LowWaterLevelThreshold { get; set; } = 30.0;
    [Range(0, 1000)]
    public double HighWaterLevelThreshold { get; set; } = 90.0;
}