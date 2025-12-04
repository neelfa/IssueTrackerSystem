namespace IssueTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";   // later: hash!
        public string Role { get; set; } = "Customer";
    }
}
