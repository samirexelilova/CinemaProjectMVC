using StreamitMVC.Models;

namespace StreamitMVC.ViewModels.TagVM
{
    public class TagVM
    {
        public List<Tag> Tags { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
