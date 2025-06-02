using AutoMapper;
using WaterSupplySimulator.Models;
using WaterSupplySimulator.DTOs;

namespace WaterSupplySimulator.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SensorReading, SensorReadingDto>().ReverseMap();
            CreateMap<PumpState, PumpStateDto>().ReverseMap();
            CreateMap<Alert, AlertDto>().ReverseMap();
            CreateMap<EventLog, EventLogDto>().ReverseMap();
        }
    }
}
