using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA_BACK_END.DTOs;
using PIA_BACK_END.Entidades;
using Microsoft.AspNetCore.Identity;
using PIA_BACK_END.Validaciones;
using Microsoft.AspNetCore.JsonPatch;

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
        private readonly ILogger<RifasController> logger;

        public RifasController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager, ILogger<RifasController> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpPost("agregar")]
        [Authorize(Policy = "esAdmin")]
        [PrimeraLetraMayuscula]
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


        [HttpGet("{idDeRifa:int}/NumerosDeLoteriaDisponibles")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetNumerosLoteriaDisponibles(int idDeRifa)
        {
            var participacionesRifaDB = await context.rifaParticipante.Where(x => x.idDeRifa == idDeRifa).ToListAsync();

            var listaNumerosDisponibles = new List<int>();
            for (int i = 1; i <= 54; i++)
            {
                listaNumerosDisponibles.Add(i);
            }

            foreach (var par in participacionesRifaDB)
            {
                foreach (var i in listaNumerosDisponibles)
                {
                    if(i == par.numeroLoteria)
                    {
                        listaNumerosDisponibles.Remove(i);
                        break;
                    }
                }
            }

            var str = "";

            foreach(var  num in listaNumerosDisponibles)
            {
                str = str + " " + num.ToString();
            }

            return str;
        }

        [HttpGet("consultar")]
        [AllowAnonymous]
        public async Task<ActionResult<List<GetRifaDTO>>> Get()
        {
            logger.LogInformation("OBTENIENDO RIFAS...");
            var rifas = await context.rifas.ToListAsync();
            return mapper.Map<List<GetRifaDTO>>(rifas);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "esAdmin")]
        public async Task<ActionResult> Put(Rifa rifa, int id)
        {
            var exist = await context.rifas.AnyAsync(x => x.Id == id);
            if (!exist)
            {
                return NotFound();
            }

            if (rifa.Id != id)
            {
                return BadRequest("El id de la rifa no coincide con el establecido en la url.");
            }

            context.Update(rifa);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<RifaPatchDTO> patchDocument)
        {
            if (patchDocument == null) { return BadRequest(); }

            var rifaDB = await context.rifas.FirstOrDefaultAsync(x => x.Id == id);

            if (rifaDB == null) { return NotFound(); }

            var rifaDTO = mapper.Map<RifaPatchDTO>(rifaDB);

            patchDocument.ApplyTo(rifaDTO);

            var isValid = TryValidateModel(rifaDTO);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(rifaDTO, rifaDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        [Authorize(Policy = "esAdmin")]
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
