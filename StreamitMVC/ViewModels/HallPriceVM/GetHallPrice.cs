namespace StreamitMVC.ViewModels
{
    public class GetHallPrice
    {
        public int Id { get; set; }
        public string HallTypeName { get; set; }
        public decimal Price { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
    }
}
