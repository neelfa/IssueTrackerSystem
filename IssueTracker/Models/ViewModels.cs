namespace IssueTracker.Models
{
    public class CreateIssueViewModel
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Priority { get; set; } = "Medium";
    }
    
    public class IssueDetailViewModel
    {
        public Issue Issue { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new();
        public string NewComment { get; set; } = "";
        public bool CanEdit { get; set; }
        public bool CanAssign { get; set; }
        public List<User> Engineers { get; set; } = new();
    }
    
    public class UserManagementViewModel
    {
        public List<User> Users { get; set; } = new();
    }
}