using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA_BACK_END.DTOs;
using PIA_BACK_END.Entidades;

namespace PIA_BACK_END.Controllers
{
    [ApiController]
    [Route("api/premios")]
    public class PremiosController  : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public PremiosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("agregar")]
        public async Task<ActionResult> Post([FromBody] PremioCreacionDTO premioCreacionDTO)
        {
            var existeRifa = await context.premios.AnyAsync(x => x.Nombre == premioCreacionDTO.Nombre);

            if (existeRifa)
            {
                return BadRequest("Ya existe un premio con ese nombre");
            }

            var rifa = mapper.Map<Premio>(premioCreacionDTO);

            context.Add(rifa);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
