using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateTagVM
    {
        [Required]
        public string Name { get; set; }
        public List<int>? SelectedMovieIds { get; set; }
        public List<Movie>? AllMovies { get; set; }
    }
}
