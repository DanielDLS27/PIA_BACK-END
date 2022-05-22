namespace PIA_BACK_END.Entidades
{
    public class RifaParticipante
    {
        public int Id { get; set; }
        public int idParticipante { get; set; }
        public int idDeRifa { get; set; }
        public int numeroLoteria { get; set; }
        public int idPremio { get; set; }
        public bool ganador { get; set; }
        public Participante participante { get; set; }
    }
}
