using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Machines
{
    public class MachineNewPageBase : ComponentBase, IDisposable
    {
        [Inject] protected MachineRepository RepoMachine { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        [SupplyParameterFromQuery]
        public int? ClientId { get; set; }

        protected Machine Machine { get; set; } = new();
        protected EditContext EditContext { get; private set; } = default!;
        protected bool IsSaving { get; private set; }
        protected string? ErrorMessage { get; private set; }

        private readonly CancellationTokenSource _cts = new();

        protected override void OnInitialized()
        {
            if (ClientId.HasValue)
            {
                Machine.ClientId = ClientId.Value;
            }
            EditContext = new EditContext(Machine);
        }

        protected async Task HandleValidSubmit()
        {
            await SaveAsync(_cts.Token).ConfigureAwait(false);
        }

        protected virtual async Task SaveAsync(CancellationToken cancellationToken)
        {
            IsSaving = true;
            ErrorMessage = null;

            try
            {
                await RepoMachine.InsertAsync(Machine, cancellationToken).ConfigureAwait(false);
                if (ClientId.HasValue)
                    Navigation.NavigateTo($"/clients/details/{ClientId.Value}");
                else
                    Navigation.NavigateTo("/machines");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Errore durante il salvataggio: " + ex.Message;
            }
            finally
            {
                IsSaving = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void Cancel()
        {
            if (ClientId.HasValue)
                Navigation.NavigateTo($"/clients/details/{ClientId.Value}");
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