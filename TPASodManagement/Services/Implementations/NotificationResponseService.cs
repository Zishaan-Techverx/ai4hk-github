using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Services;

namespace TpaSodManagement.Services.Implementations;

public class NotificationResponseService : INotificationResponseService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationResponseService> _logger;

    public NotificationResponseService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<NotificationResponseService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ServiceResponse<NotificationResponse>> CreateResponseAsync(long notificationId, long userId, string reply)
    {
        var response = new ServiceResponse<NotificationResponse>();
        try
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                response.Success = false;
                response.Message = "Reply cannot be empty.";
                return response;
            }

            // Check if user has already replied to this notification
            var existingReply = await _context.NotificationResponses
                .FirstOrDefaultAsync(nr => nr.NotificationId == notificationId 
                    && nr.UserId == userId 
                    && nr.DeletedDate == null);

            if (existingReply != null)
            {
                response.Success = false;
                response.Message = "You are not allowed to send multiple Replies";
                return response;
            }

            var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
            var notificationResponse = new NotificationResponse
            {
                NotificationId = notificationId,
                UserId = userId,
                Reply = reply.Trim(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByUserId = currentUserId
            };

            _context.NotificationResponses.Add(notificationResponse);
            await _context.SaveChangesAsync();

            response.Data = notificationResponse;
            response.Success = true;
            response.Message = "Reply sent successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification response for NotificationId: {NotificationId}, UserId: {UserId}", notificationId, userId);
            response.Success = false;
            response.Message = $"Error sending reply: {ex.Message}";
        }
        return response;
    }

    public async Task<List<NotificationResponse>> GetResponsesByNotificationIdAsync(long notificationId)
    {
        return await _context.NotificationResponses
            .Include(nr => nr.User)
            .Where(nr => nr.NotificationId == notificationId && nr.DeletedDate == null)
            .OrderByDescending(nr => nr.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<NotificationResponse>> GetResponsesByUserIdAsync(long userId)
    {
        return await _context.NotificationResponses
            .Include(nr => nr.Notification)
            .Where(nr => nr.UserId == userId)
            .OrderByDescending(nr => nr.CreatedDate)
            .ToListAsync();
    }
}

