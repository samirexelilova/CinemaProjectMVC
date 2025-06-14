using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class GetPositionVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Actor>? Actors { get; set; }
    }
}
