using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class CategoryIndexVM
    {
        public List<Category> Categories { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
