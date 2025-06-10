using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Subtitle:BaseEntity
    {
            public int MovieId { get; set; }
            public Movie Movie { get; set; }

            public int LanguageId { get; set; }
            public Language Language { get; set; }

            public string FilePath { get; set; }  // Serverdə saxlanılan subtitr faylının yolu 
        }

    }
