using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class CreateTagVM
    {
        [Required]
        public string Name { get; set; }
    }
}
