using SmartRoom.Models;

namespace SmartRoom.DTOs;

public class DashboardStatsDto
{
    public int TotalActiveRooms { get; set; }
    public int TodayBookingsCount { get; set; }
    public int PendingBookingsCount { get; set; }
    public int ApprovedBookingsCount { get; set; }
    public List<Booking> TodayBookings { get; set; } = new();
}
