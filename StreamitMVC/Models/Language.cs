using StreamitMVC.Models.Base;
using System.Security.Cryptography;

namespace StreamitMVC.Models
{
    public class Language:BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; } 
        public string FlagIcon { get; set; } // Bayraq ikonunun şəkili
        public List<Movie> Movies { get; set; }  // Bu dildə olan filmlər
        public List<Subtitle> Subtitles { get; set; }  // Bu dildə olan bütün altyazılar
        public List<Session> Sessions { get; set; } 
    }
}
