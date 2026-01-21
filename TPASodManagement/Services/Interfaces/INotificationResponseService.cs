using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services;

namespace TpaSodManagement.Services.Interfaces;

public interface INotificationResponseService
{
    Task<ServiceResponse<NotificationResponse>> CreateResponseAsync(long notificationId, long userId, string reply);
    Task<List<NotificationResponse>> GetResponsesByNotificationIdAsync(long notificationId);
    Task<List<NotificationResponse>> GetResponsesByUserIdAsync(long userId);
}

