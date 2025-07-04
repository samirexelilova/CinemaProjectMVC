using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }

        public decimal OriginalPrice { get; set; }  
        public decimal SalePrice { get; set; }      

        public string ImageUrl { get; set; } 

        public int CategoryId { get; set; }
        public ProductCategory Category { get; set; }

        public int Stock { get; set; } = 100;
        public bool HasDiscount => SalePrice < OriginalPrice;
        public decimal DiscountAmount => HasDiscount ? OriginalPrice - SalePrice : 0;
        public decimal Price => HasDiscount ? SalePrice : OriginalPrice;
    }
}
