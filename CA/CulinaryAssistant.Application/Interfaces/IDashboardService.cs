using CulinaryAssistant.Application.DTOs;

namespace CulinaryAssistant.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса Dashboard (Application Layer)
/// </summary>
public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}
