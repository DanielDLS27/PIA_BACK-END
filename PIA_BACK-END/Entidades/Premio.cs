namespace PIA_BACK_END.Entidades
{
    public class Premio
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool disponibilidad { get; set; }
        public int idRifa { get; set; }
        public int orden { get; set; }
        public Rifa rifa { get; set; }
    }
}
