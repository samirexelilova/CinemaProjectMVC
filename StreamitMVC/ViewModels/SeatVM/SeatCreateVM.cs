using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class SeatCreateVM
    {
        public int HallId { get; set; }
        public string HallName { get; set; }

        [Required(ErrorMessage = "Sıra nömrəsi tələb olunur")]
        [Range(1, int.MaxValue, ErrorMessage = "Sıra nömrəsi 1-dən böyük olmalıdır")]
        public int RowNumber { get; set; }

        [Required(ErrorMessage = "Oturacaq nömrəsi tələb olunur")]
        [Range(1, int.MaxValue, ErrorMessage = "Oturacaq nömrəsi 1-dən böyük olmalıdır")]
        public int SeatNumber { get; set; }

        [Required(ErrorMessage = "Oturacaq tipi seçilməlidir")]
        public int SeatTypeId { get; set; }
        public List<SeatType>? SeatTypes { get; set; } 
    }
}
