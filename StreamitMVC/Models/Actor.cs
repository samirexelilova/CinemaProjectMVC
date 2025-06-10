using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Actor:BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int PositionId { get; set; }
        public Position Position { get; set; }
        public string Description { get; set; }
        public string Award { get; set; }
        public string? Facebook { get; set; }
        public string? Twitter { get; set; }
        public string? Github { get; set; }
        public string? Instagram { get; set; }
        public string Born { get; set; }
        public string Parents { get; set; }
        public List<MovieActor> MovieActors { get; set; }


    }
}
