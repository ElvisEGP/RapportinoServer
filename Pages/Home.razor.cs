using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;
using RapportinoServer.Services;

namespace RapportinoServer.Pages
{
    public class HomePageBase : ComponentBase
    {
        [Inject] protected DashboardRepository DashboardRepository { get; set; } = default!;
        [Inject] protected AuthStateService AuthState { get; set; } = default!;
        [Inject] protected ILogger<HomePageBase> Logger { get; set; } = default!;

        protected DashboardSummary? Summary { get; private set; }
        protected bool IsLoading { get; private set; } = true;
        protected string? LoadError { get; private set; }

        protected int MaxServiceSlice { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Summary = await DashboardRepository.GetSummaryAsync().ConfigureAwait(false);
                MaxServiceSlice = Summary.ServiceTypeBreakdown.Count == 0
                    ? 0
                    : Summary.ServiceTypeBreakdown.Max(s => s.ItemCount);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore caricamento dashboard");
                LoadError = "Impossibile caricare le metriche. Verificare la connessione al database.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected static int BarPercent(int value, int max)
        {
            if (max <= 0)
                return 0;
            return (int)Math.Clamp(Math.Round(value * 100.0 / max), 0, 100);
        }
    }
}
