using System.ComponentModel.DataAnnotations;
using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class CreateCategoryVM
    {
        [Required]
        public string Name { get; set; }
        public List<int>? SelectedMovieIds { get; set; }
        public List<Movie>? AllMovies { get; set; }
    }
}
