using Restaurant.Domain.Results;

namespace Restaurant.Domain.WorkingHours;

public static class WorkingHourErrors
{
    public static readonly Error InvalidBranch = Error.Validation("WorkingHour.Branch.Required", "Branch is required.");
    public static readonly Error InvalidTimeRange = Error.Validation("WorkingHour.Time.Invalid", "Close time must be after open time.");
    public static readonly Error NotFound = Error.NotFound("WorkingHour.NotFound", "Working hour was not found.");
}