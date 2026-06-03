using Onboard.Web.Domain;

namespace Onboard.Web.Models;

public sealed class DashboardViewModel
{
    public int TotalCases { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
    public IReadOnlyCollection<OnboardingCase> RecentCases { get; set; } = [];
    public string SwaggerUrl { get; set; } = "/swagger";
}
