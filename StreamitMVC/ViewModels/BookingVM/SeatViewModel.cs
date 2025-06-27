namespace StreamitMVC.ViewModels
{
    public class SeatViewModel
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Number { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public bool IsTaken { get; set; }
        public bool IsSold { get; set; }
        public bool IsReserved { get; set; }
        public decimal Price { get; set; }
    }
}
