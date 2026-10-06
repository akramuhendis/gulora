using Gulora.Application.Common.Configuration;
using Gulora.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gulora.API.Services;

/// <summary>
/// Development: bazada qalmış <c>Pending</c> rəyləri bir dəfə təsdiq edir və məhsul reytinq saylarını yeniləyir.
/// </summary>
public sealed class DevApprovePendingReviewsHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevApprovePendingReviewsHostedService> _logger;
    private readonly ReviewPublishOptions _options;

    public DevApprovePendingReviewsHostedService(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<DevApprovePendingReviewsHostedService> logger,
        IOptions<ReviewPublishOptions> options)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment() || !_options.DevApprovePendingReviewsOnStartup)
            return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var reviewRepository = scope.ServiceProvider.GetRequiredService<IReviewRepository>();
            var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var pending = await reviewRepository.GetPendingAsync(cancellationToken);
            if (pending.Count == 0)
                return;

            foreach (var review in pending)
                review.Approve();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productIds = pending.Select(r => r.ProductId).Distinct().ToList();
            foreach (var productId in productIds)
            {
                var (avg, count) = await reviewRepository.GetRatingStatsAsync(productId, cancellationToken);
                var product = await productRepository.GetByIdAsync(productId, cancellationToken);
                product?.RecalculateRating(avg, count);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "DevApprovePendingReviews: {Count} gözləmədə rəy təsdiqləndi və məhsul reytinqləri yeniləndi.",
                pending.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DevApprovePendingReviews: xəta — atlandı.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
