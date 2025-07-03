using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class ActorVm
    {
        public List<Actor> Actors { get; set; }
        public List<Movie> Movies { get; set; } 
        public List<MovieActor> MovieActors { get; set; }
        public List<Movie> MostViewedMovies { get; set; } 
        public Actor Actor { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
