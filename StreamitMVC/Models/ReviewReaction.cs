using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class ReviewReaction:BaseEntity
    {

        public int ReviewId { get; set; }
        public Review Review { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public bool IsLike { get; set; }
    }
}
