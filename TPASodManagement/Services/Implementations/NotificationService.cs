using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Services;

namespace TpaSodManagement.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<List<Notification>> GetAllNotificationsAsync()
    {
        return await _context.Notifications
            .Include(n => n.NotificationUsers)
            .ThenInclude(nu => nu.User)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<Notification?> GetNotificationByIdAsync(long id)
    {
        return await _context.Notifications
            .Include(n => n.NotificationUsers)
            .ThenInclude(nu => nu.User)
            .FirstOrDefaultAsync(n => n.NotificationId == id);
    }

    public async Task<ServiceResponse<Notification>> CreateNotificationAsync(Notification notification, List<long> userIds)
    {
        var response = new ServiceResponse<Notification>();
        try
        {
            var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
            notification.CreatedDate = DateTimeOffset.UtcNow;
            notification.CreatedByUserId = currentUserId;
            notification.IsActive = true;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Create NotificationUser entries for selected users
            foreach (var userId in userIds)
            {
                var notificationUser = new NotificationUser
                {
                    NotificationId = notification.NotificationId,
                    UserId = userId,
                    IsRead = false,
                    CreatedDate = DateTimeOffset.UtcNow,
                    CreatedByUserId = currentUserId
                };
                _context.NotificationUsers.Add(notificationUser);
            }

            await _context.SaveChangesAsync();
            response.Data = notification;
            response.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            response.Success = false;
            response.Message = $"Error creating notification: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<Notification>> UpdateNotificationAsync(Notification notification, List<long> userIds)
    {
        var response = new ServiceResponse<Notification>();
        try
        {
            var existingNotification = await _context.Notifications
                .Include(n => n.NotificationUsers)
                .FirstOrDefaultAsync(n => n.NotificationId == notification.NotificationId);

            if (existingNotification == null)
            {
                response.Success = false;
                response.Message = "Notification not found";
                return response;
            }

            existingNotification.Title = notification.Title;
            existingNotification.Message = notification.Message;
            existingNotification.Priority = notification.Priority;
            existingNotification.ExpiryDate = notification.ExpiryDate;
            existingNotification.IsActive = notification.IsActive;

            var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
            existingNotification.UpdatedDate = DateTimeOffset.UtcNow;
            existingNotification.UpdatedByUserId = currentUserId;

            // Update NotificationUser relationships
            if (userIds != null && userIds.Count > 0)
            {
                // Get current user IDs
                var currentUserIds = existingNotification.NotificationUsers
                    .Where(nu => nu.DeletedDate == null)
                    .Select(nu => nu.UserId)
                    .ToList();

                // Find users to remove (in current but not in new list)
                var usersToRemove = existingNotification.NotificationUsers
                    .Where(nu => nu.DeletedDate == null && !userIds.Contains(nu.UserId))
                    .ToList();

                // Soft delete removed users
                foreach (var notificationUser in usersToRemove)
                {
                    notificationUser.DeletedDate = DateTimeOffset.UtcNow;
                    notificationUser.DeletedByUserId = currentUserId;
                }

                // Find users to add (in new list but not in current)
                var usersToAdd = userIds
                    .Where(userId => !currentUserIds.Contains(userId))
                    .ToList();

                // Add new users or restore soft-deleted users
                foreach (var userId in usersToAdd)
                {
                    // Check if a soft-deleted entry exists for this user (ignore query filters to find soft-deleted)
                    var existingSoftDeleted = await _context.NotificationUsers
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(nu => nu.NotificationId == existingNotification.NotificationId 
                            && nu.UserId == userId 
                            && nu.DeletedDate != null);

                    if (existingSoftDeleted != null)
                    {
                        // Restore the soft-deleted entry
                        existingSoftDeleted.DeletedDate = null;
                        existingSoftDeleted.DeletedByUserId = null;
                        existingSoftDeleted.IsRead = false; // Reset read status
                        existingSoftDeleted.UpdatedDate = DateTimeOffset.UtcNow;
                        existingSoftDeleted.UpdatedByUserId = currentUserId;
                    }
                    else
                    {
                        // Check if entry already exists (to avoid duplicate key error)
                        var existingEntry = await _context.NotificationUsers
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(nu => nu.NotificationId == existingNotification.NotificationId 
                                && nu.UserId == userId);

                        if (existingEntry == null)
                        {
                            // Create new entry only if it doesn't exist
                            var notificationUser = new NotificationUser
                            {
                                NotificationId = existingNotification.NotificationId,
                                UserId = userId,
                                IsRead = false,
                                CreatedDate = DateTimeOffset.UtcNow,
                                CreatedByUserId = currentUserId
                            };
                            _context.NotificationUsers.Add(notificationUser);
                        }
                    }
                }
            }

            // Mark all active users as unread when notification is updated
            // This ensures users see the updated notification content
            // Query from context to include newly added users
            var activeNotificationUsers = await _context.NotificationUsers
                .Where(nu => nu.NotificationId == existingNotification.NotificationId 
                    && nu.DeletedDate == null)
                .ToListAsync();

            foreach (var notificationUser in activeNotificationUsers)
            {
                notificationUser.IsRead = false;
                notificationUser.ReadDate = null;
                notificationUser.UpdatedDate = DateTimeOffset.UtcNow;
                notificationUser.UpdatedByUserId = currentUserId;
            }

            await _context.SaveChangesAsync();
            response.Data = existingNotification;
            response.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification");
            response.Success = false;
            response.Message = $"Error updating notification: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<bool>> DeleteNotificationAsync(long id)
    {
        var response = new ServiceResponse<bool>();
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.DeletedDate == null);

            if (notification == null)
            {
                response.Success = false;
                response.Message = "Notification not found";
                return response;
            }

            var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
            notification.DeletedDate = DateTimeOffset.UtcNow;
            notification.DeletedByUserId = currentUserId;

            await _context.SaveChangesAsync();
            response.Data = true;
            response.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            response.Success = false;
            response.Message = $"Error deleting notification: {ex.Message}";
        }
        return response;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(long userId)
    {
        return await _context.NotificationUsers
            .Where(nu => nu.UserId == userId && nu.DeletedDate == null)
            .Include(nu => nu.Notification)
            .Where(nu => nu.Notification.DeletedDate == null && nu.Notification.IsActive)
            .Select(nu => nu.Notification)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(long notificationId, long userId)
    {
        try
        {
            var notificationUser = await _context.NotificationUsers
                .FirstOrDefaultAsync(nu => nu.NotificationId == notificationId && nu.UserId == userId);

            if (notificationUser != null && !notificationUser.IsRead)
            {
                notificationUser.IsRead = true;
                notificationUser.ReadDate = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return false;
        }
    }

    public async Task<List<Notification>> GetNotificationsByOrganizationAsync(long? organizationId)
    {
        if (!organizationId.HasValue)
        {
            return await GetAllNotificationsAsync();
        }

        return await _context.NotificationUsers
            .Include(nu => nu.User)
            .Include(nu => nu.Notification)
            .Where(nu => nu.User.OrganizationId == organizationId.Value && nu.DeletedDate == null)
            .Where(nu => nu.Notification.DeletedDate == null && nu.Notification.IsActive)
            .Select(nu => nu.Notification)
            .Distinct()
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }
}

