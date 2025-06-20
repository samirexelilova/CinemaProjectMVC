﻿using StreamitMVC.Models;
using System.ComponentModel.DataAnnotations;

namespace StreamitMVC.ViewModels
{
    public class GetCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Movie>? Movies { get; set; }
    }
}
