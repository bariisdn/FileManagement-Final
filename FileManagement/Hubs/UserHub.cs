using Microsoft.AspNetCore.SignalR;

namespace FileManagement.Hubs
{
    public class UserHub : Hub
    {
        public async Task UpdateUserRole(string userId, string newRole)
        {
            // Notify all clients about the user role update
            await Clients.All.SendAsync("UserUpdatedRole", userId, newRole);
        }

        public async Task SendFileNotification(string fileName, string userId)
        {
            // Notify all clients about a new file creation
            await Clients.All.SendAsync("FileCreated", fileName, userId);
        }

        public async Task SendFileDeletedNotification(string fileName, string userId)
        {
            // Notify all clients about a file deletion
            await Clients.All.SendAsync("FileDeleted", fileName, userId);
        }

        public async Task SendNotification(string message)
        {
            // Send a general notification message to all clients
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}