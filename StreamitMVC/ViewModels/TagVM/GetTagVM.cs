using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class GetTagVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Movie>? Movies { get; set; }
    }
}
