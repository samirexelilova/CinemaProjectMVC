
using StreamitMVC.Models;

namespace StreamitMVC.ViewModels.CreateMovieVM
{
    public class CreateMovieVM
    {
        public string Name { get; set; }
        public IFormFile Photo { get; set; }
        public IFormFile TrailerVideoFile { get; set; }
        public IFormFile VideoUrlFile { get; set; }
        public TimeSpan Duration { get; set; }
        public string Director { get; set; }
        public string Country { get; set; }
        public decimal ImdbRating { get; set; }
        public string Description { get; set; }
        public int AgeRestriction { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int LanguageId { get; set; }

        public List<int>? SelectedCategoryIds { get; set; }
        public List<int>? SelectedTagIds { get; set; }
        public List<int>? SelectedActorIds { get; set; }

        public List<Category> AllCategories { get; set; }
        public List<Tag> AllTags { get; set; }
        public List<Actor> AllActors { get; set; }
        public List<Language> AllLanguages { get; set; }
    }

}
