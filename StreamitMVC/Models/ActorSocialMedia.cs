using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models.Base;

namespace StreamitMVC.Models
{
    public class ActorSocialMedia:BaseEntity
    {
        public int ActorId { get; set; }
        public Actor Actor { get; set; }
        public SocialMediaPlatform Platform { get; set; }  // Enum
        public string Url { get; set; }
    }
}
