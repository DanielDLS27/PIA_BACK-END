using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PIA_BACK_END.Entidades;

namespace PIA_BACK_END
{
    public class ApplicationDbContext: IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Rifa> rifas { get; set; }
        public DbSet<Participante> participantes { get; set; }
        public DbSet<Premio> premios { get; set; }
        public DbSet<RifaParticipante> rifaParticipante { get; set; }
    }
}
