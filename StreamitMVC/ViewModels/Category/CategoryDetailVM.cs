using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class CategoryDetailVM
    {
        public Category Category { get; set; }
        public List<Movie> Movies { get; set; }
    }
}
