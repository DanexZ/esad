namespace WebApi
{
    public class ExtendedEntitiesInput : EntitiesInput
    {
        public DateTime dataOd { get; set; }
        public DateTime dataDo { get; set; }
        public int kryteriumFiltrowania { get; set; }
        public string filtrSlowny { get; set; }
    }
}
