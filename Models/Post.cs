using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SocialX.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(140)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public IdentityUser? User { get; set; }

        [NotMapped]
        public bool CanEdit =>
            DateTime.UtcNow - CreatedAt <= TimeSpan.FromMinutes(5);
    }
}
