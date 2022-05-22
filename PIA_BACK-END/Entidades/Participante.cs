namespace PIA_BACK_END.Entidades
{
    public class Participante
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public List<RifaParticipante> Participaciones { get; set; }
    }
}
