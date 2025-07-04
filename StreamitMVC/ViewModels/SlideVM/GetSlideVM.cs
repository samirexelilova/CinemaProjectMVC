using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class GetSlideVM
    {
        public int Id { get; set; }

        [MaxLength(50, ErrorMessage = "Title 50 simvoldan az olmalidir")]
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Order minimum 1 olmalidir")]
        public int Order { get; set; }
        public string Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TrailerVideo { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal ImdbRating { get; set; }
        public DateTime ReleaseDate { get; set; } //filmin ne zaman cixdigi tarix
        public string Description { get; set; }
    }
}
