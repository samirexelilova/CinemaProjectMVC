using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class TagDetailVM
    {
        public Tag Tag { get; set; }
        public List<Movie> Movies { get; set; }
    }
}
