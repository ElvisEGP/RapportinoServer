using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Machines
{
    public class MachineEditPageBase : ComponentBase, IDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected MachineRepository RepoMachine { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected ILogger<MachineEditPageBase> Logger { get; set; } = default!;

        protected Machine? Machine { get; set; }
        protected EditContext EditContext { get; private set; } = default!;
        protected bool IsLoading { get; private set; } = true;
        protected bool IsSaving { get; private set; }
        protected string? ErrorMessage { get; private set; }
        protected string? LoadError { get; private set; }

        private readonly CancellationTokenSource _cts = new();

        protected override async Task OnParametersSetAsync()
        {
            await LoadMachineAsync(_cts.Token).ConfigureAwait(false);
        }

        private async Task LoadMachineAsync(CancellationToken cancellationToken)
        {
            IsLoading = true;
            LoadError = null;
            ErrorMessage = null;

            try
            {
                Machine = await RepoMachine.GetByIdAsync(Id, cancellationToken).ConfigureAwait(false);

                if (Machine is not null)
                {
                    EditContext = new EditContext(Machine);
                }
                else
                {
                    LoadError = null;
                }
            }
            catch (OperationCanceledException)
            {
                LoadError = "Caricamento annullato.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante il caricamento della macchina {MachineId}", Id);
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
            await SaveAsync(_cts.Token).ConfigureAwait(false);
        }

        protected virtual async Task SaveAsync(CancellationToken cancellationToken)
        {
            if (Machine is null)
            {
                ErrorMessage = "Macchina non disponibile.";
                return;
            }

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                await RepoMachine.UpdateAsync(Machine, cancellationToken).ConfigureAwait(false);
                Navigation.NavigateTo($"/clients/details/{Machine.ClientId}");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante il salvataggio della macchina {MachineId}", Id);
                ErrorMessage = "Errore durante il salvataggio.";
            }
            finally
            {
                IsSaving = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void Cancel()
        {
            if (Machine != null)
                Navigation.NavigateTo($"/clients/details/{Machine.ClientId}");
            else
                Navigation.NavigateTo("/machines");
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}