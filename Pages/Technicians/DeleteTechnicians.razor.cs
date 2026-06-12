using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Technicians
{
    public class DeleteTechniciansPageBase : ComponentBase, IAsyncDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected TechnicianRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime Js { get; set; } = default!;
        [Inject] protected ILogger<DeleteTechniciansPageBase> Logger { get; set; } = default!;

        protected Technician? Technician { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected bool IsDeleting { get; set; }
        protected string? LoadError { get; set; }
        protected string? ErrorMessage { get; set; }

        private CancellationTokenSource? _cts;

        protected override async Task OnParametersSetAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsLoading = true;
            LoadError = null;
            ErrorMessage = null;
            Technician = null;

            try
            {
                Technician = await Repo.GetByIdAsync(Id, _cts.Token).ConfigureAwait(false);

                if (Technician is null)
                    LoadError = null;
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore caricamento tecnico {TechnicianId}", Id);
                LoadError = "Errore durante il caricamento. Riprova più tardi.";
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task ConfirmAsync()
        {
            if (Technician is null)
            {
                ErrorMessage = "Tecnico non disponibile per l'eliminazione.";
                return;
            }

            if (IsDeleting)
                return;

            IsDeleting = true;
            ErrorMessage = null;

            try
            {
                var confirmed = await Js.InvokeAsync<bool>(
                    "confirm",
                    $"Eliminare il tecnico '{Technician.TechnicianName}'?"
                );

                if (!confirmed)
                    return;

                await Repo.DeleteAsync(Id, _cts?.Token ?? CancellationToken.None).ConfigureAwait(false);
                Nav.NavigateTo("/technicians");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante l'eliminazione del tecnico {TechnicianId}", Id);
                ErrorMessage = "Errore durante l'eliminazione.";
            }
            finally
            {
                IsDeleting = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void Cancel() => Nav.NavigateTo("/technicians");

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}