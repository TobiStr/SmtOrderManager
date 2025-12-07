namespace SmtOrderManager.Domain.Enums;

/// <summary>
/// Represents the status of an order in the SMT manufacturing system.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is being drafted and can be modified.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Order has been submitted for processing.
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Order is currently in production.
    /// </summary>
    InProduction = 2,

    /// <summary>
    /// Order has been completed.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Order has been archived.
    /// </summary>
    Archived = 5
}
