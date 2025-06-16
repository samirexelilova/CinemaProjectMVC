using Microsoft.AspNetCore.Mvc.Rendering;
using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class CreateHallVM
    {
        public string Name { get; set; }
        public int CinemaId { get; set; }
        public int Capacity { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
        public int HallTypeId { get; set; }

        public List<Cinema>? Cinemas { get; set; }
        public List<HallType>? HallTypes { get; set; }
    }
}
