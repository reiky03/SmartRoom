using SmartRoom.DTOs;
using SmartRoom.Models;

namespace SmartRoom.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetAllRoomsAsync(bool? isActive = null);
    Task<Room?> GetRoomByIdAsync(int id);
    Task<Room?> GetRoomByCodeAsync(string roomCode);
    Task<Room> CreateRoomAsync(Room room);
    Task<Room?> UpdateRoomAsync(int id, Room room);
    Task<bool> ToggleRoomStatusAsync(int id);
    Task<bool> DeleteRoomAsync(int id);
    Task<RoomAvailabilityDto?> GetRoomAvailabilityAsync(int roomId, DateOnly date);
}
