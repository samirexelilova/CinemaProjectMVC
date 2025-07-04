using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
        public class UpdateMovieVM
        {
            public string Name { get; set; }

            public string PhotoPath { get; set; }
            public string TrailerVideoPath { get; set; }
            public string VideoPath { get; set; }

            public IFormFile? PhotoFile { get; set; }
            public IFormFile? TrailerVideo { get; set; }
            public IFormFile? VideoFile { get; set; }
        public decimal? DirectPurchasePrice { get; set; }
        public bool IsAvailableForDirectPurchase { get; set; }
        public TimeSpan Duration { get; set; }
            public string Director { get; set; }
            public string Country { get; set; }
            public decimal ImdbRating { get; set; }
            public string Description { get; set; }
            public int AgeRestriction { get; set; }
            public DateTime ReleaseDate { get; set; }

            public List<int>? SelectedCategoryIds { get; set; }
            public List<int>? SelectedTagIds { get; set; }
            public List<int>? SelectedLanguageIds { get; set; }
            public List<int>? SelectedSubtitleIds { get; set; }

            public List<Category>? Categories { get; set; }
            public List<Tag>? Tags { get; set; }
            public List<Actor>? Actors { get; set; }
            public List<Language>? Languages { get; set; }
            public List<Subtitle>? Subtitles { get; set; }

            public List<ActorWithRole>? ActorRoles { get; set; }

            public class ActorWithRole
            {
                public int ActorId { get; set; }
                public string Role { get; set; }
            }
        }
    }
