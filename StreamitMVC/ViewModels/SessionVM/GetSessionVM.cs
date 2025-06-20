namespace StreamitMVC.ViewModels
{
    public class GetSessionVM
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string MovieName { get; set; }
        public string CinemaName { get; set; }
        public string HallName { get; set; }
        public string LanguageName { get; set; }
        public string SubtitleName { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
    }
}
