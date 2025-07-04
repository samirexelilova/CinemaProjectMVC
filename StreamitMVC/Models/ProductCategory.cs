using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class ProductCategory:BaseEntity
    {
        public string Name { get; set; }
        public List<Product> Products { get; set; }
    }
}
