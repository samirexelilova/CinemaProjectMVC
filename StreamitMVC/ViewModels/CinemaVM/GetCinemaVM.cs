namespace StreamitMVC.ViewModels
{
    public class GetCinemaVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }
}
