using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class SeatEditVM
    {
        public int HallId { get; set; }
        public string HallName { get; set; }

        [Display(Name = "Sıra nömrəsi")]
        public int RowNumber { get; set; }

        [Display(Name = "Oturacaq nömrəsi")]
        public int SeatNumber { get; set; }

        [Required(ErrorMessage = "Oturacaq tipi seçilməlidir")]
        [Display(Name = "Oturacaq tipi")]
        public int SeatTypeId { get; set; }

        public List<SeatType>? SeatTypes { get; set; } 
    }
}
