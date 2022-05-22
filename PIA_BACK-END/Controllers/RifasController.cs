using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA_BACK_END.DTOs;
using PIA_BACK_END.Entidades;
using Microsoft.AspNetCore.Identity;

namespace PIA_BACK_END.Controllers
{
    [ApiController]
    [Route("api/rifas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RifasController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public RifasController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpPost("agregar")]
        public async Task<ActionResult> Post([FromBody] RifaCreacionDTO rifaCreacionDTO)
        {
            var existeRifa = await context.rifas.AnyAsync(x => x.Nombre == rifaCreacionDTO.Nombre);

            if (existeRifa)
            {
                return BadRequest("Ya existe una rifa con ese nombre");
            }

            var rifa = mapper.Map<Rifa>(rifaCreacionDTO);

            context.Add(rifa);
            await context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpPost("registrarParticipante/{idRifa:int}/boleto")]
        public async Task<ActionResult> PostRegistrarParticipante(int idRifa)
        {
            var existeRifa = await context.rifas.AnyAsync(x => x.Id == idRifa);
            if (!existeRifa)
            {
                return BadRequest("No existe la rifa");
            }
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();

            var participante = await context.participantes.FirstOrDefaultAsync(x => x.Email.Equals(emailClaim.Value));

            var isNumberValid = false;

            var numBoleto = 0;
            var rand = new Random();

            while (!isNumberValid)
            {
                isNumberValid = true;
                var numBoletoRandom = rand.Next(1, 54);

                var numerosUsados = await context.rifaParticipante.Where(x => x.idDeRifa == idRifa).ToListAsync();
                if (numerosUsados.Count == 54) return BadRequest("Ya no hay numeros disponibles");

                foreach (var numero in numerosUsados)
                {
                    if(numero.numeroLoteria == numBoletoRandom)
                    {
                        isNumberValid = false;
                        break;
                    }
                }
                numBoleto = numBoletoRandom;
            }

            var rifaParticipante = new RifaParticipante()
            {
                idParticipante = participante.Id,
                idDeRifa = idRifa,
                numeroLoteria = numBoleto,
                ganador = false,
                participante = participante
            };

            context.Add(rifaParticipante);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{idDeRifa:int}/ObtenerGanador")]
        [Authorize(Policy = "esAdmin")]
        public async Task<ActionResult<Object>> getGanador(int idDeRifa)
        {
            var rifa = await context.rifas.FirstOrDefaultAsync(x => x.Id == idDeRifa);
            if (rifa == null) return BadRequest("Rifa incorrecta");
            
            var participaciones = await context.rifaParticipante.
                Where(x => x.idDeRifa == idDeRifa && x.ganador == false).ToListAsync();

            if (participaciones.Count == 0) return BadRequest("No hay participaciones en la rifa");

            Random random = new Random();
            var ganador = participaciones.
                OrderBy(x => random.Next()).
                Take(1).FirstOrDefault();

            ganador.ganador = true;
            context.rifaParticipante.Update(ganador);
            await context.SaveChangesAsync();

            var datosParticipante = await context.participantes.Where(x => x.Id == ganador.idParticipante).FirstOrDefaultAsync();

            var boletoGanador = new
            {
                numero = ganador.numeroLoteria,
                email = datosParticipante.Email
            };

            return boletoGanador;
        }   

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<RifaDTO>> GetById(int id)
        {
            var rifa = await context.rifas.FirstOrDefaultAsync(x => x.Id == id);

            if (rifa == null)
            {
                return NotFound();
            }

            return mapper.Map<RifaDTO>(rifa);
        }

        [HttpGet("{nombre}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RifaDTO>>> GetByNombre([FromRoute] string nombre)
        {
            var rifas = await context.rifas.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
            if(rifas.Count == 0)
            {
                return NotFound();
            }
            return mapper.Map<List<RifaDTO>>(rifas);
        }

        [HttpGet("consultar")]
        [AllowAnonymous]
        public async Task<ActionResult<List<GetRifaDTO>>> Get()
        {
            var rifas = await context.rifas.ToListAsync();
            return mapper.Map<List<GetRifaDTO>>(rifas);
        }

        

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await context.rifas.AnyAsync(x => x.Id == id);
            if (!exist)
            {
                return NotFound();
            }
            context.Remove(new Rifa { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }

    }
}
