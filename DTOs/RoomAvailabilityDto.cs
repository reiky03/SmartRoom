namespace SmartRoom.DTOs;

public class BookedSlotDto
{
    public string BookingReference { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RoomAvailabilityDto
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public List<BookedSlotDto> BookedSlots { get; set; } = new();
}
