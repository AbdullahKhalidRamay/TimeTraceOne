namespace TimeTraceOne.Models;

public enum UserRole
{
    employee,
    manager,
    owner
}

public enum EntryStatus
{
    Pending,
    Approved,
    Rejected
}

public enum ProjectType
{
    TimeAndMaterial,
    FixedCost,
    Retainer,
    Internal
}

public enum ProjectStatus
{
    Active,
    Completed,
    OnHold,
    Cancelled
}

public enum NotificationType
{
    Approval,
    Rejection,
    Reminder,
    System
}
