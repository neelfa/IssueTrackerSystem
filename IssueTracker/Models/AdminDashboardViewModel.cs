using System.Collections.Generic;

namespace IssueTracker.Models
{
    public class AdminDashboardViewModel
    {
        // issue stats
        public int TotalIssues { get; set; }
        public int OpenIssues { get; set; }
        public int InProgressIssues { get; set; }
        public int ClosedIssues { get; set; }

        // user stats
        public int CustomersCount { get; set; }
        public int EngineersCount { get; set; }
        public int AdminsCount { get; set; }

        // recent issues
        public List<Issue> RecentIssues { get; set; } = new();
    }
}
