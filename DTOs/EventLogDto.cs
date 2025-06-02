namespace WaterSupplySimulator.DTOs;

public class EventLogDto
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
