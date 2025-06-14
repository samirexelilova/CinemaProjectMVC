using StreamitMVC.Models;
using StreamitMVC.ViewModels.ActorVM;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateActorVM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        public IFormFile? Photo { get; set; }
        public string Image { get; set; }
        [Required]
        public int? PositionId { get; set; }
        public List<Position>? Positions { get; set; }
        public string Description { get; set; }
        public string Award { get; set; }
        public DateTime? Born { get; set; }
        public string Parents { get; set; }
        public List<Movie>? Movies { get; set; }
        public List<SocialMediaVM>? SocialMedias { get; set; }
    }

}
