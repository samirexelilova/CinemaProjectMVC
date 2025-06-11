using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }
        public List<MovieCategory> MovieCategories { get; set; }
    }
}
