namespace CalyxAttendanceManagement.Client.Services.AuthService
{
    public interface IAuthService
    {
        event Action OnChange;
        List<User> Users { get; set; }
        Task<ServiceResponse<int>> Register(UserRegister request);
        Task<ServiceResponse<string>> Login(UserLogin request);
        Task<ServiceResponse<bool>> ChangePassword(UserChangePassword request);
        Task<bool> IsUserAuthenticated();
        Task<User> GetUser();
        Task GetUsers();
        Task<ServiceResponse<bool>> UpdateProfile(UpdateProfile request);
    }
}
