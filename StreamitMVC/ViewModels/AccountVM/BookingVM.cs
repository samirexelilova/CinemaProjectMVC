﻿using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;

namespace StreamitMVC.ViewModels;

public class BookingViewModel
{
    public int Id { get; set; }
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int TicketCount { get; set; }

    public Session? Session { get; set; } 
    public string SessionName { get; set; }
    public DateTime SessionDate { get; set; }
}
