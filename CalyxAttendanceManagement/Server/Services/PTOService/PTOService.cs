using CalyxAttendanceManagement.Shared;

namespace CalyxAttendanceManagement.Server.Services.PTOService;

public class PTOService : IPTOService
{
    private readonly DataContext _context;
    private readonly IAuthService _authService;

    public PTOService(DataContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<ServiceResponse<IList<UserPTOHistory>>> GetPTOHistories()
    {
        int userId = _authService.GetUserId();

        var userPTOHistories = await _context.UserPTOHistory.Where(a => a.UserId == userId).ToListAsync();
        
        return new ServiceResponse<IList<UserPTOHistory>> { Data = userPTOHistories };
    }

    public async Task<ServiceResponse<decimal>> GetPTOCount()
    {
        int userId = _authService.GetUserId();

        var userPTOCount = await _context.UserPTO.Where(a => a.UserId == userId).Select(a => a.Pto).FirstOrDefaultAsync();

        return new ServiceResponse<decimal> { Data = userPTOCount };
    }

}
