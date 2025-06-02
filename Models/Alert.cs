using System.ComponentModel.DataAnnotations;

namespace WaterSupplySimulator.Models;

public class Alert
{
    public int Id { get; set; }
    [Required]
    public string Type { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsAcknowledged { get; set; }
}