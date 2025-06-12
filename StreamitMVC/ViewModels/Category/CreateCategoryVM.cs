using StreamitMVC.Models;

namespace StreamitMVC.ViewModels.Category
{
    public class CreateCategoryVM
    {
        public string Name { get; set; }
        public List<Movie>? AllMovies { get; set; }
    }
}
