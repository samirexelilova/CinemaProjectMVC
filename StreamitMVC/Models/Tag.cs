using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Tag:BaseEntity
    {
        public string Name { get; set; }
        public List<MovieTag> MovieTags { get; set; }
    }
}
