namespace IssueTracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByEmail { get; set; } = "";
        public int IssueId { get; set; }
        
        // Navigation properties
        public virtual Issue Issue { get; set; } = null!;
    }
}