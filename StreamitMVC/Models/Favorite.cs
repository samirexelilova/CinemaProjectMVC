﻿using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class Favorite:BaseEntity
    {
        public string UserId { get; set; }       
        public AppUser User { get; set; }

        public int MovieId { get; set; }        
        public Movie Movie { get; set; }

    }
}
