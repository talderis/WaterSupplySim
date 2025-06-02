namespace WaterSupplySimulator.DTOs;

public class SensorReadingDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double WaterLevel { get; set; }
    public double Inflow { get; set; }
    public double Outflow { get; set; }
    public bool PowerState { get; set; }
}
