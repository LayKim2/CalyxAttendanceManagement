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

        var userPTOHistories = await _context.UserPTOHistory.Where(a => a.UserId == userId).OrderByDescending(a => a.Id).ToListAsync();
        
        return new ServiceResponse<IList<UserPTOHistory>> { Data = userPTOHistories };
    }

    public async Task<ServiceResponse<decimal>> GetPTOCount()
    {
        int userId = _authService.GetUserId();

        var userPTOCount = await _context.UserPTO.Where(a => a.UserId == userId).Select(a => a.Pto).FirstOrDefaultAsync();

        return new ServiceResponse<decimal> { Data = userPTOCount };
    }

    public async Task<ServiceResponse<bool>> RequestPTO(UserRequestPTO requestPTO)
    {
        try
        {
            int userId = _authService.GetUserId();

            var userPTO = await _context.UserPTO.Where(up => up.UserId == userId).FirstOrDefaultAsync();

            if (userPTO != null)
            {
                decimal needPTO = 0;

                switch (requestPTO.PTOType)
                {
                    case "1일 이상":
                        needPTO = (requestPTO.EndDate.Value.Day - requestPTO.StartDate.Value.Day + 1);
                        break;
                    case "1일":
                        requestPTO.EndDate = requestPTO.StartDate;
                        needPTO = 1;
                        break;
                    case "오전반차":
                    case "오후반차":
                        requestPTO.EndDate = requestPTO.StartDate;
                        needPTO = 0.5m;
                        break;
                    default:
                        requestPTO.EndDate = requestPTO.StartDate;
                        needPTO = 0.25m;
                        break;
                }

                var userPTOHistory = new UserPTOHistory()
                {
                    UserId = userId,
                    UserPTOId = userPTO.Id,
                    PTOType = requestPTO.PTOType,
                    StartDate = requestPTO.StartDate,
                    EndDate = requestPTO.EndDate,
                    NeedPTO = needPTO,
                    CurrentPTO = userPTO.Pto,
                    CalculatedPTO = userPTO.Pto - needPTO,
                    Comment = requestPTO.Comment,
                    CreatedDate = DateTime.Now,
                    VerifiedType = "Pending"
                };

                _context.Add(userPTOHistory);

                await _context.SaveChangesAsync();

                return new ServiceResponse<bool> { Data = true };
            } else
            {
                return new ServiceResponse<bool> { Data = false, Success = false, Message = "User do now exist." };
            }
        }
        catch (Exception e)
        {
            return new ServiceResponse<bool> { Data = false, Success = false, Message = e.Message };
        }

        
    }

}
