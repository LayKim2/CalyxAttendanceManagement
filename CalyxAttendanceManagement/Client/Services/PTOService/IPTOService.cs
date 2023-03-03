namespace CalyxAttendanceManagement.Client.Services.PTOService;

public interface IPTOService
{
    event Action OnChange;

    IList<UserPTOHistory> UserPTOHistories { get; set; }
    decimal UserPTOCount { get; set; }

    Task GetUserPTOHistories();

    Task GetPTOCount();
}
