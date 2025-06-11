using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Position:BaseEntity
    {
        public string Name { get; set; }
        public List<Actor> Actors { get; set; }
    }
}
