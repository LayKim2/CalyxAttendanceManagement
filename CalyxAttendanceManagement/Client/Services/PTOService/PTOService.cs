using CalyxAttendanceManagement.Client.Pages.User;

namespace CalyxAttendanceManagement.Client.Services.PTOService;

public class PTOService : IPTOService
{
    private readonly HttpClient _http;

    public IList<UserPTOHistory> UserPTOHistories { get; set; } = new List<UserPTOHistory>();
    public decimal UserPTOCount { get; set; }

    public PTOService(HttpClient http)
    {
        _http = http;
    }

    public event Action OnChange;

    public async Task GetUserPTOHistories()
    {
        var response = await _http.GetFromJsonAsync<ServiceResponse<IList<UserPTOHistory>>>("api/pto/get-histories");

        if (response.Success)
            UserPTOHistories = response.Data;
    }

    public async Task GetPTOCount()
    {
        var response = await _http.GetFromJsonAsync<ServiceResponse<decimal>>("api/pto/get-pto-count");

        UserPTOCount = response.Data;
    }

}
