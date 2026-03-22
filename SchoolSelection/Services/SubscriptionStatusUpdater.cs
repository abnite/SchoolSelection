using SchoolSelection.Data;

namespace SchoolSelection.Services;

public class SubscriptionStatusUpdater:IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer _timer;
    
    public SubscriptionStatusUpdater(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    private void UpdateSubscriptions(object state)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CollegeDbContext>();

        var expiredSubscriptions = dbContext.Subscriptions
            .Where(s => s.EndDate <= DateTime.Now && s.IsActive)
            .ToList();

        foreach (var subscription in expiredSubscriptions)
        {
            subscription.IsActive = false;
        }

        dbContext.SaveChanges();
    }
    
    public  Task StartAsync(CancellationToken cancellationToken)
    {
        // Run the task every 1 hour (or adjust as needed)
        _timer = new Timer(UpdateSubscriptions, null, TimeSpan.Zero, TimeSpan.FromHours(24));
        return Task.CompletedTask;
    }

    public  Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}