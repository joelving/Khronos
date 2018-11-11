namespace Khronos.Shared
{
    public enum EventStatus
    {
        // Event status
        Tentative,
        Confirmed,

        // Todo status
        NeedsAction,
        Completed,
        InProgress,

        // Journal status
        Draft,
        Final,
        
        // Common status
        Cancelled,
    }
}
