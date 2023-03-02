
namespace CalyxAttendanceManagement.Shared.ViewModel;

public class UpdateUserPTO
{
    public int UserId { get; set; }
    public int UserPTOHistoryId { get; set; }
    public bool Result { get; set; }
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
