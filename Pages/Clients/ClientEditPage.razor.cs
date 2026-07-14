using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RapportinoServer.Data.Repositories; // ajuste conforme seu namespace real
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Clients
{
    public class ClientEditPageBase : ComponentBase, IDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected ClientRepository RepoClient { get; set; } = default!;
        [Inject] protected MachineRepository RepoMachine { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected IJSRuntime Js { get; set; } = default!;

        protected Client? Client { get; set; }
        protected EditContext EditContext { get; private set; } = default!;
        protected bool IsLoading { get; private set; } = true;
        protected bool IsSaving { get; private set; }
        protected string? ErrorMessage { get; private set; }
        protected string? LoadError { get; private set; }

        private readonly CancellationTokenSource _cts = new();

        protected DateTime DataAsDateTime
        {
            get => Client is null || Client.Data == default ? DateTime.Today : Client.Data;
            set
            {
                if (Client is not null)
                    Client.Data = value;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!_cts.Token.IsCancellationRequested)
                await LoadClientAsync(_cts.Token);
        }

        private async Task LoadClientAsync(CancellationToken cancellationToken)
        {
            IsLoading = true;
            LoadError = null;
            try
            {
                Client = await RepoClient.GetByIdAsync(Id, cancellationToken);
                if (Client is not null)
                    EditContext = new EditContext(Client);
                else
                    LoadError = "Cliente non trovato.";
            }
            catch (OperationCanceledException)
            {
                LoadError = "Caricamento annullato.";
            }
            catch (Exception ex)
            {
                LoadError = "Errore durante il caricamento: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
                // StateHasChanged deve ser invocato dentro di InvokeAsync per evitare InvalidOperationException
                await InvokeAsync(() =>
                {
                    try
                    {
                        StateHasChanged();
                    }
                    catch
                    {
                        // Component potrebbe essere stato unloaded
                    }
                });
            }
        }

        protected async Task HandleValidSubmit()
        {
            await SaveAsync(_cts.Token);
        }

        protected virtual async Task SaveAsync(CancellationToken cancellationToken)
        {
            if (Client is null) return;

            IsSaving = true;
            ErrorMessage = null;
            try
            {
                if (Client.Data == default)
                    Client.Data = DateTime.UtcNow;

                await RepoClient.UpdateAsync(Client, cancellationToken);

                Navigation.NavigateTo("/clients");
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
                // StateHasChanged deve essere invocato dentro di InvokeAsync per evitare InvalidOperationException
                await InvokeAsync(() =>
                {
                    try
                    {
                        StateHasChanged();
                    }
                    catch
                    {
                        // Component potrebbe essere stato unloaded
                    }
                });
            }
        }

        protected void GoBack() => Navigation.NavigateTo("/clients");

        protected void AddMachine() => Navigation.NavigateTo($"/machines/new?clientId={Id}");

        protected void EditMachine(int machineId) => Navigation.NavigateTo($"/machines/edit/{machineId}");

        protected async Task DeleteMachine(int machineId)
        {
            if (await Js.InvokeAsync<bool>("confirm", "Eliminare questa macchina?"))
            {
                try
                {
                    await RepoMachine.DeleteAsync(machineId);
                    await LoadClientAsync(_cts.Token);
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Errore durante l'eliminazione della macchina: " + ex.Message;
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}