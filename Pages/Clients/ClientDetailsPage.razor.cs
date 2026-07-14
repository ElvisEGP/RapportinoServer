using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Clients
{
    public class ClientDetailsPageBase : ComponentBase
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected ClientRepository RepoClient { get; set; } = default!;
        [Inject] protected MachineRepository RepoMachine { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected IJSRuntime Js { get; set; } = default!;

        protected Client? Client { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected string? LoadError { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await LoadClientAsync();
        }

        private async Task LoadClientAsync()
        {
            IsLoading = true;
            LoadError = null;

            try
            {
                Client = await RepoClient.GetByIdAsync(Id, CancellationToken.None).ConfigureAwait(false);
                if (Client is null)
                {
                    LoadError = null;
                }
            }
            catch (Exception ex)
            {
                LoadError = "Errore durante il caricamento del cliente: " + ex.Message;
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

        protected void BackToList() => Navigation.NavigateTo("/clients");

        protected void EditClient() => Navigation.NavigateTo($"/clients/edit/{Id}");

        protected void AddMachine() => Navigation.NavigateTo($"/machines/new?clientId={Id}");

        protected void EditMachine(int machineId) => Navigation.NavigateTo($"/machines/edit/{machineId}");

        protected async Task DeleteMachine(int machineId)
        {
            if (await Js.InvokeAsync<bool>("confirm", "Eliminare questa macchina?"))
            {
                try
                {
                    await RepoMachine.DeleteAsync(machineId);
                    await LoadClientAsync();
                }
                catch (Exception ex)
                {
                    LoadError = "Errore durante l'eliminazione della macchina: " + ex.Message;
                }
            }
        }
    }
}
