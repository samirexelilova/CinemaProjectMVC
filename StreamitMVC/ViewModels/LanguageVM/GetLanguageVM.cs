using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class GetLanguageVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string FlagImage { get; set; }
        public List<Movie>? Movies { get; set; }
    }
}
