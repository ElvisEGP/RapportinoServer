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
    public class NewTypeServicePageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected TypeServiceRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<NewTypeServicePageBase> Logger { get; set; } = default!;

        protected TypeService Service { get; set; } = new TypeService();
        protected EditContext EditContext { get; private set; } = default!;
        protected bool IsSaving { get; private set; }
        protected string? ErrorMessage { get; private set; }

        private CancellationTokenSource? _cts;

        protected override void OnInitialized()
        {
            EditContext = new EditContext(Service);
        }

        protected async Task HandleValidSubmit()
        {
            if (IsSaving) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                // validazione minima: nome non vuoto
                if (string.IsNullOrWhiteSpace(Service.Name))
                {
                    ErrorMessage = "Inserisci un nome valido.";
                    return;
                }

                await Repo.InsertAsync(Service, _cts.Token).ConfigureAwait(false);
                Nav.NavigateTo("/services");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
                Logger.LogInformation("Creazione tipo di servizio annullata.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante la creazione del tipo di servizio");
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
