using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WaterSupplySimulator.Data;
using WaterSupplySimulator.Models;

namespace WaterSupplySimulator.Services;

public class SensorDataService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random = new();

    public SensorDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WaterSystemContext>();

        // Initialize settings and pump state
        if (!context.ControlSettings.Any())
        {
            context.ControlSettings.Add(new ControlSettings());
            context.PumpStates.Add(new PumpState { IsOn = false, Mode = "Auto", LastChanged = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var reading = new SensorReading
            {
                Timestamp = DateTime.UtcNow,
                WaterLevel = _random.NextDouble() * 100,
                Inflow = _random.NextDouble() * 10,
                Outflow = _random.NextDouble() * 8,
                PowerState = _random.Next(0, 10) > 1
            };

            context.SensorReadings.Add(reading);

            // Pump control logic
            var settings = context.ControlSettings.First();
            var pump = context.PumpStates.First();
            if (pump.Mode == "Auto")
            {
                if (reading.WaterLevel < settings.LowWaterLevelThreshold && !pump.IsOn)
                {
                    pump.IsOn = true;
                    pump.LastChanged = DateTime.UtcNow;
                    context.EventLogs.Add(new EventLog
                    {
                        EventType = "Pump",
                        Message = "Pump turned ON (Auto)",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else if (reading.WaterLevel > settings.HighWaterLevelThreshold && pump.IsOn)
                {
                    pump.IsOn = false;
                    pump.LastChanged = DateTime.UtcNow;
                    context.EventLogs.Add(new EventLog
                    {
                        EventType = "Pump",
                        Message = "Pump turned OFF (Auto)",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            // Alert logic
            if (reading.WaterLevel < settings.LowWaterLevelThreshold)
            {
                context.Alerts.Add(new Alert
                {
                    Type = "LowLevel",
                    Message = "Water level below threshold",
                    CreatedAt = DateTime.UtcNow,
                    IsAcknowledged = false
                });
            }
            else if (reading.WaterLevel > settings.HighWaterLevelThreshold)
            {
                context.Alerts.Add(new Alert
                {
                    Type = "Overflow",
                    Message = "Water level above threshold",
                    CreatedAt = DateTime.UtcNow,
                    IsAcknowledged = false
                });
            }
            if (!reading.PowerState)
            {
                context.Alerts.Add(new Alert
                {
                    Type = "SensorFailure",
                    Message = "Power outage detected",
                    CreatedAt = DateTime.UtcNow,
                    IsAcknowledged = false
                });
            }

            await context.SaveChangesAsync();
            await Task.Delay(5000, stoppingToken);
        }
    }
}