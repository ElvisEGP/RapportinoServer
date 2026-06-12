using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Clients
{
    public class ClientNewPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected ClientRepository Repo { get; set; } = default!;
        [Inject] protected MachineRepository RepoMachine { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<ClientNewPageBase> Logger { get; set; } = default!;

        protected Client Client { get; set; } = new Client
        {
            CompanyName = string.Empty,
            Address = string.Empty,
            NumberAddress = string.Empty,
            City = string.Empty,
            PostalCode = string.Empty,
            State = string.Empty,
            Country = string.Empty,
            Phone = string.Empty,
            Email = string.Empty
        };

        protected EditContext EditContext { get; private set; } = default!;
        protected bool IsSaving { get; private set; }
        protected string? ErrorMessage { get; private set; }

        private CancellationTokenSource? _cts;

        protected override void OnInitialized()
        {
            if (Client.Machines.Count == 0)
                Client.Machines.Add(new Machine());

            EditContext = new EditContext(Client);
        }

        protected void AddMachine()
        {
            Client.Machines.Add(new Machine());
        }

        protected void RemoveMachine(Machine machine)
        {
            Client.Machines.Remove(machine);

            if (Client.Machines.Count == 0)
                Client.Machines.Add(new Machine());
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
                if (string.IsNullOrWhiteSpace(Client.CompanyName))
                {
                    ErrorMessage = "Inserisci la ragione sociale.";
                    return;
                }

                // Validazione: almeno 1 macchina con serial
                var validMachines = Client.Machines.Where(m => !string.IsNullOrWhiteSpace(m.Serial)).ToList();
                if (validMachines.Count == 0)
                {
                    ErrorMessage = "Aggiungi almeno una macchina con il numero di matricola.";
                    return;
                }

                var clientId = await Repo.InsertAsync(Client, _cts.Token).ConfigureAwait(false);

                foreach (var machine in validMachines)
                {
                    machine.ClientId = clientId;
                    await RepoMachine.InsertAsync(machine, _cts.Token).ConfigureAwait(false);
                }

                Nav.NavigateTo("/clients");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
                Logger.LogInformation("Creazione cliente annullata.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante la creazione del cliente");
                ErrorMessage = "Errore durante il salvataggio: " + ex.Message;
            }
            finally
            {
                IsSaving = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void Cancel() => Nav.NavigateTo("/clients");

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}