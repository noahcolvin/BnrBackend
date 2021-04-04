using System.ComponentModel.DataAnnotations;

namespace BnrBackend.Models
{
    public class Post
    {
        public int Id { get; set; }
        [Required]
        public User User { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
    }
}