namespace StreamitMVC.ViewModels
{
    public class MovieFilterVM
    {
        public string SearchText { get; set; }
        public int? LanguageId { get; set; }
        public int? CinemaId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? Date { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
    }
}
