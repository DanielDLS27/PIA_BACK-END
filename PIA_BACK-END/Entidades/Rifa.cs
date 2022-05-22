namespace PIA_BACK_END.Entidades
{
    public class Rifa
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool vigencia { get; set; }
        public List<RifaParticipante> participaciones { get; set; }
        public List<Premio> premios { get; set; }
    }
}
