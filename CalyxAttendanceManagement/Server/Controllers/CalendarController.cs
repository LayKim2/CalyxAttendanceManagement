using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Radzen.Blazor.Rendering;

namespace CalyxAttendanceManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IList<Calendar>>>> GetCalendar(int id)
        {
            return await _calendarService.GetCalendar();
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<bool>>> AddorCalendar(Calendar calendar)
        {
            return await _calendarService.AddCalendar(calendar);
        }
    }
}
