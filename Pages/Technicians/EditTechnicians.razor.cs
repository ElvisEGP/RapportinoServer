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
    public class EditTechniciansPageBase : ComponentBase, IAsyncDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected TechnicianRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<EditTechniciansPageBase> Logger { get; set; } = default!;

        protected Technician? Technician { get; set; }
        protected EditContext EditContext { get; private set; } = default!;
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
            Technician = null;

            try
            {
                Technician = await Repo.GetByIdAsync(Id, _cts.Token).ConfigureAwait(false);

                if (Technician is null)
                {
                    LoadError = null;
                    return;
                }

                EditContext = new EditContext(Technician);
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

        protected async Task HandleValidSubmit()
        {
            if (IsSaving || Technician is null)
                return;

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                await Repo.UpdateAsync(Technician, _cts?.Token ?? CancellationToken.None).ConfigureAwait(false);
                Nav.NavigateTo("/technicians");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante il salvataggio del tecnico {TechnicianId}", Id);
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