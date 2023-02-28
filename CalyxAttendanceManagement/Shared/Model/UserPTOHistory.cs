
namespace CalyxAttendanceManagement.Shared.Model;

public class UserPTOHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TypeCD { get; set; }
    public string Type { get; set; }
    public int before { get; set; }
    public int after { get; set; }
    public int total { get; set; }
    public string comment { get; set; }
}
