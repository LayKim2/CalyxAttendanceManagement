using SendGrid;
using SendGrid.Helpers.Mail;

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

    private async Task<bool> RequestEmail(SendEmail request, UserPTOHistory userPTOHistory)
    {
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("koreaus1@naver.com", "Calyx Attendance Management");
        var to = new EmailAddress(request.Email, request.Name);
        var subject = "PTO 신청";
        var plainTextContent = "";
        var datehtml = "";

        if(userPTOHistory.PTOType == "1일 이상")
        {
            datehtml = userPTOHistory.StartDate.Value.ToString("MM/dd/yyyy") + "~"  + userPTOHistory.EndDate.Value.ToString("MM/dd/yyyy");
        } else
        {
            datehtml = userPTOHistory.StartDate.Value.ToString("MM/dd/yyyy");
        };

        var htmlContent = $"Hi, Wayne <br/></br/> 신청자 : {request.Name}, {request.Email} <br/><br/> PTO Type : {userPTOHistory.PTOType} <br/><br/> Date : {datehtml} <br/><br/> {userPTOHistory.Comment} <br/></br/></br/> Thank you";
        
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        
        var response = await client.SendEmailAsync(msg);

        return true;
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

                // send email
                var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

                if(user != null)
                {
                    await RequestEmail(new SendEmail { Email = user.Email, Name = user.Name }, userPTOHistory);
                }

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
