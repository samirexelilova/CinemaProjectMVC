using System.ComponentModel.DataAnnotations;
using StreamitMVC.Models;

namespace StreamitMVC.ViewModels
{
    public class CreateCategoryVM
    {
        [Required]
        public string Name { get; set; }
    }
}
