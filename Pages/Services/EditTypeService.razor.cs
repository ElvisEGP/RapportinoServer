using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Services
{
    public class EditTypeServicePageBase : ComponentBase, IAsyncDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected TypeServiceRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<EditTypeServicePageBase> Logger { get; set; } = default!;

        protected TypeService? Service { get; set; }
        protected EditContext? EditContext { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected bool IsSaving { get; set; }
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
            Service = null;

            try
            {
                Service = await Repo.GetByIdAsync(Id, _cts.Token).ConfigureAwait(false);

                if (Service is null)
                {
                    LoadError = "Tipo di servizio non trovato.";
                    return;
                }

                EditContext = new EditContext(Service);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore caricamento tipo di servizio {TypeServiceId}", Id);
                LoadError = "Errore durante il caricamento. Riprova più tardi.";
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task HandleValidSubmit()
        {
            if (Service is null) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                await Repo.UpdateAsync(Service, _cts.Token).ConfigureAwait(false);
                Nav.NavigateTo("/services");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
                Logger.LogInformation("Aggiornamento tipo di servizio annullato {TypeServiceId}", Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante l'aggiornamento del tipo di servizio {TypeServiceId}", Id);
                ErrorMessage = "Errore durante il salvataggio: " + ex.Message;
            }
            finally
            {
                IsSaving = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void Cancel() => Nav.NavigateTo("/services");

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}
