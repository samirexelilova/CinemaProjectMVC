using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class UpdateTagVM
    {
        [Required]
        public string Name { get; set; }
    }
}
