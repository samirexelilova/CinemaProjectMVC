namespace StreamitMVC.ViewModels
{
    public class SeatDeleteVM
    {
        public int Id { get; set; }
        public int HallId { get; set; }
        public string HallName { get; set; }
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public string SeatTypeName { get; set; }
    }
}
