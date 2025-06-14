using StreamitMVC.Models;
using StreamitMVC.ViewModels.ActorVM;

namespace StreamitMVC.ViewModels
{
    public class GetActorVM
    {
        public int Id { get; set; }
        public string FullName => $"{Name} {Surname}";
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Photo { get; set; }
        public string PositionName { get; set; }
        public string Description { get; set; }
        public string Award { get; set; }
        public DateTime? Born { get; set; }
        public string Parents { get; set; }
        public int FilmCount { get; set; } 
        public List<SocialMediaVM> SocialMedias { get; set; }
    }
}
