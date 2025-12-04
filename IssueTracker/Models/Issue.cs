namespace IssueTracker.Models
{
    public class Issue
    {
        public int Id { get; set; }                      // primary key
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Priority { get; set; } = "Medium"; // Low / Medium / High
        public string Status { get; set; } = "Open";     // Open / InProgress / Resolved
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByEmail { get; set; } = ""; // simple link to user
    }
}
