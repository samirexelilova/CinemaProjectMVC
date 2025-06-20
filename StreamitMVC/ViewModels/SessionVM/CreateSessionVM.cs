using Microsoft.AspNetCore.Mvc.Rendering;
using StreamitMVC.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class CreateSessionVM
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int MovieId { get; set; }

        public int CinemaId { get; set; }

        public int HallId { get; set; }

        public int HallPriceId { get; set; }

        public int LanguageId { get; set; }

        public List<Movie>? Movies { get; set; } 
        public List<Cinema>? Cinemas { get; set; } 
        public List<Hall>? Halls { get; set; } 
        public List<HallPrice>? HallPrices { get; set; } 
        public List<Language>? Languages { get; set; }

        public List<Subtitle>? Subtitles { get; set; }

        public int? SubtitleId { get; set; }  // Seçilmiş altyazı
    }
}
