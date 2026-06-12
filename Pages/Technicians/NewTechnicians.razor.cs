using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Technicians
{
    public class NewTechnicianPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected TechnicianRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<NewTechnicianPageBase> Logger { get; set; } = default!;

        protected Technician Technician { get; set; } = new();
        protected EditContext EditContext { get; private set; } = default!;
        protected bool IsSaving { get; private set; }
        protected string? ErrorMessage { get; private set; }

        private CancellationTokenSource? _cts;

        protected override void OnInitialized()
        {
            EditContext = new EditContext(Technician);
        }

        protected async Task HandleValidSubmit()
        {
            if (IsSaving)
                return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                await Repo.InsertAsync(Technician, _cts.Token).ConfigureAwait(false);
                Nav.NavigateTo("/technicians");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante il salvataggio del tecnico");
                ErrorMessage = "Errore durante il salvataggio.";
            }
            finally
            {
                IsSaving = false;
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