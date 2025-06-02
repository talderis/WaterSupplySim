using System.ComponentModel.DataAnnotations;

namespace WaterSupplySimulator.Models;

public class EventLog
{
    public int Id { get; set; }
    [Required]
    public string EventType { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}