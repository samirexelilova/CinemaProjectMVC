namespace StreamitMVC.Extensions.Enums
{
    public enum RefundStatus
    {
        Pending, // Geri ödəmə gözləmədədir, hələ təsdiqlənməyib

        Approved,// Geri ödəmə təsdiqlənib, həyata keçirilir

        Rejected ,// Geri ödəmə rədd edilib
        Completed

    }
}
