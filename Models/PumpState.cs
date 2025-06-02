using System.ComponentModel.DataAnnotations;

namespace WaterSupplySimulator.Models;

public class PumpState
{
    public int Id { get; set; }
    public bool IsOn { get; set; }
    [Required]
    [RegularExpression("Auto|Manual", ErrorMessage = "Mode must be 'Auto' or 'Manual'.")]
    public string Mode { get; set; } = "Auto";
    public DateTime LastChanged { get; set; }
}