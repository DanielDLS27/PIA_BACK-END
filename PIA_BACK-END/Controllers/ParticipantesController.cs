using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA_BACK_END.DTOs;
using PIA_BACK_END.Entidades;

namespace PIA_BACK_END.Controllers
{
    [ApiController]
    [Route("api/participantes")]
    public class ParticipantesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ParticipantesController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("crear")]
        public async Task<ActionResult> Post([FromBody] ParticipanteCreacionDTO participanteCreacionDTO)
        {
            var existeParticipante = await context.participantes.AnyAsync(x => x.Email.Equals(participanteCreacionDTO.Email));

            if (existeParticipante)
            {
                return BadRequest("Ya existe un participante con ese email");
            }

            var rifa = mapper.Map<Participante>(participanteCreacionDTO);

            context.Add(rifa);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
