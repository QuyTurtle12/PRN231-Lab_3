namespace DataAccess.DTO.Orchid
{
    public class BaseOrchidDTO
    {
        public bool? IsNatural { get; set; }

        public string? OrchidDescription { get; set; }

        public string? OrchidName { get; set; }

        public string? OrchidUrl { get; set; }

        public decimal? Price { get; set; }

        public int? CategoryId { get; set; }
    }
}
