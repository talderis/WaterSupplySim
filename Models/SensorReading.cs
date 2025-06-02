using System.ComponentModel.DataAnnotations;

namespace WaterSupplySimulator.Models;

public class SensorReading
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    [Range(0, 1000, ErrorMessage = "WaterLevel must be between 0 and 1000.")]
    public double WaterLevel { get; set; }
    [Range(0, 500, ErrorMessage = "Inflow must be between 0 and 500.")]
    public double Inflow { get; set; }
    [Range(0, 500, ErrorMessage = "Outflow must be between 0 and 500.")]
    public double Outflow { get; set; }
    public bool PowerState { get; set; }
}