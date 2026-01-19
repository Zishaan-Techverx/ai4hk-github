using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services;

namespace TpaSodManagement.Services.Interfaces;

public interface INotificationService
{
    Task<List<Notification>> GetAllNotificationsAsync();
    Task<Notification?> GetNotificationByIdAsync(long id);
    Task<ServiceResponse<Notification>> CreateNotificationAsync(Notification notification, List<long> userIds);
    Task<ServiceResponse<Notification>> UpdateNotificationAsync(Notification notification, List<long> userIds);
    Task<ServiceResponse<bool>> DeleteNotificationAsync(long id);
    Task<List<Notification>> GetUserNotificationsAsync(long userId);
    Task<bool> MarkAsReadAsync(long notificationId, long userId);
    Task<List<Notification>> GetNotificationsByOrganizationAsync(long? organizationId);
}

