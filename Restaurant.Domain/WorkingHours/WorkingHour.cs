using Restaurant.Domain.Common;
using Restaurant.Domain.Results;

namespace Restaurant.Domain.WorkingHours;

public sealed class WorkingHour : AuditableEntity
{
    public Guid BranchId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan OpenTime { get; private set; }
    public TimeSpan CloseTime { get; private set; }
    public bool IsClosed { get; private set; } // إجازة

    public Branches.Branch Branch { get; private set; } = null!;

    private WorkingHour() { }

    private WorkingHour(Guid id, Guid branchId, DayOfWeek dayOfWeek,
        TimeSpan openTime, TimeSpan closeTime, bool isClosed) : base(id)
    {
        BranchId = branchId; DayOfWeek = dayOfWeek;
        OpenTime = openTime; CloseTime = closeTime; IsClosed = isClosed;
    }

    public static Result<WorkingHour> Create(Guid id, Guid branchId, DayOfWeek dayOfWeek,
        TimeSpan openTime, TimeSpan closeTime, bool isClosed = false)
    {
        if (branchId == Guid.Empty) return WorkingHourErrors.InvalidBranch;
        if (!isClosed && openTime >= closeTime) return WorkingHourErrors.InvalidTimeRange;

        return new WorkingHour(id, branchId, dayOfWeek, openTime, closeTime, isClosed);
    }

    public Result<Updated> Update(TimeSpan openTime, TimeSpan closeTime, bool isClosed)
    {
        if (!isClosed && openTime >= closeTime) return WorkingHourErrors.InvalidTimeRange;
        OpenTime = openTime; CloseTime = closeTime; IsClosed = isClosed;
        return Result.Updated;
    }
}