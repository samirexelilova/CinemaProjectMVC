using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class ShopViewModel
    {
      
            public List<Product> Products { get; set; }
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int PageSize { get; set; }
            public int TotalProducts { get; set; }

            public Basket Basket { get; set; }  


    }
}
