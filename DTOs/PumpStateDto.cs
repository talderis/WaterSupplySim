namespace WaterSupplySimulator.DTOs;

public class PumpStateDto
{
    public int Id { get; set; }
    public bool IsOn { get; set; }
    public string Mode { get; set; } = "Auto";
    public DateTime LastChanged { get; set; }
}
