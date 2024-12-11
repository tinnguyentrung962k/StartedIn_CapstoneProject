using Google.Apis.Calendar.v3;

namespace StartedIn.Service.Services.Interface;

public interface IGoogleMeetService
{
    public CalendarService GetCalendarService();
}