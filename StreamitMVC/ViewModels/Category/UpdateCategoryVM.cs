using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateCategoryVM
    {
        [Required]
        public string Name { get; set; }

    }
}
