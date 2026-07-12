using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Reports
{
    public class DetailsReportPageBase : ComponentBase, IAsyncDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected ReportRepository RepoReport { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<DetailsReportPageBase> Logger { get; set; } = default!;

        protected Report? Report { get; set; }
        protected Client? Client { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected string? LoadError { get; set; }
        protected string? DownloadError { get; set; }
        protected bool ShowDownloadModal { get; set; }

        [Inject] protected ClientRepository RepoClient { get; set; } = default!;

        private CancellationTokenSource? _cts;

        protected override async Task OnParametersSetAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsLoading = true;
            LoadError = null;
            Report = null;
            Client = null;

            try
            {
                Report = await RepoReport.GetByIdAsync(Id, _cts.Token);

                if (Report is null)
                {
                    LoadError = "Rapportino non trovato.";
                }
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore caricamento rapportino {ReportId}", Id);
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

        protected async Task PrintAsync()
        {
            await JS.InvokeVoidAsync("window.print");
        }

        protected void ShowPdfModal()
        {
            ShowDownloadModal = true;
        }

        protected void ClosePdfModal()
        {
            ShowDownloadModal = false;
        }

        protected async Task ConfirmDownloadPdf()
        {
            ShowDownloadModal = false;
            DownloadError = null;

            if (Report is null)
                return;

            try
            {
                await JS.InvokeVoidAsync("pdfInterop.downloadReport", Report.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore download PDF rapportino {ReportId}", Report.Id);
                DownloadError = "Impossibile scaricare il PDF in questo momento.";
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async Task DownloadPdfAsync()
        {
            DownloadError = null;
            ShowDownloadModal = true;
            await InvokeAsync(StateHasChanged);
        }

        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected void Back() => Nav.NavigateTo("/reports");

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}
