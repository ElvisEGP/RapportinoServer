using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Clients
{
    public class ClientDetailsPageBase : ComponentBase
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected ClientRepository RepoClient { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

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
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void BackToList() => Navigation.NavigateTo("/clients");
    }
}
