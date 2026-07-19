
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Reports
{
    public class ListReportsPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected ReportRepository RepoReport { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime Js { get; set; } = default!;
        [Inject] protected ILogger<ListReportsPageBase> Logger { get; set; } = default!;

        protected List<Report>? Reports { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected bool IsDeleting { get; set; }
        protected string? LoadError { get; set; }

        private CancellationTokenSource? _cts;

        protected override async Task OnInitializedAsync()
        {
            await LoadReportsAsync();
        }

        private async Task LoadReportsAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsLoading = true;
            LoadError = null;
            Reports = null;

            try
            {
                var all = await RepoReport.GetAllAsync(_cts.Token);
                Reports = all.ToList();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore caricamento rapportini");
                LoadError = $"Errore durante il caricamento: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(() =>
                {
                    try
                    {
                        StateHasChanged();
                    }
                    catch
                    {
                        // Component may have been unloaded while the async operation was running.
                    }
                });
            }
        }

        protected void NewReport()
        {
            Logger.LogInformation("Navegando para /reports/new");
            Nav.NavigateTo("/reports/new", forceLoad: true);
        }


        protected void GoToDetails(int id)
        {
            Nav.NavigateTo($"/reports/details/{id}");
        }

        protected async Task DeleteReport(int id)
        {
            if (IsDeleting) return;

            try
            {
                var confirmed = await Js.InvokeAsync<bool>("confirm", $"Eliminare il rapportino #{id}?");

                if (!confirmed) return;

                IsDeleting = true;

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                await RepoReport.DeleteAsync(id, _cts.Token).ConfigureAwait(false);

                // reload list
                var all = await RepoReport.GetAllAsync(_cts.Token).ConfigureAwait(false);
                Reports = all.ToList();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore eliminazione rapportino {ReportId}", id);
                // keep UI responsive; optionally set a user-visible message
            }
            finally
            {
                IsDeleting = false;
                await InvokeAsync(() =>
                {
                    try
                    {
                        StateHasChanged();
                    }
                    catch
                    {
                        // Component may have been unloaded while the async operation was running.
                    }
                });
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}