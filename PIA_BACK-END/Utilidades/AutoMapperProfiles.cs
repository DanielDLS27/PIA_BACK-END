using AutoMapper;
using PIA_BACK_END.DTOs;
using PIA_BACK_END.Entidades;

namespace PIA_BACK_END.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<RifaCreacionDTO, Rifa>();
            CreateMap<Rifa, RifaDTO>();
            CreateMap<Rifa, GetRifaDTO>();
            CreateMap<PremioCreacionDTO, Premio>();
            CreateMap<ParticipanteCreacionDTO, Participante>();
            CreateMap<ModificarRifaDTO, Rifa>();
            CreateMap<RifaPatchDTO, Rifa>().ReverseMap();
        }
    }
}
