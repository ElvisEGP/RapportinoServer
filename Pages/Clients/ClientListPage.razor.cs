using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Clients
{
    public class ClientListPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected ClientRepository RepoClient { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected IJSRuntime Js { get; set; } = default!;
        [Inject] protected ILogger<ClientListPageBase> Logger { get; set; } = default!;

        protected IEnumerable<Client> Clients { get; private set; } = Array.Empty<Client>();
        protected bool IsLoading { get; private set; } = true;
        protected string? LoadError { get; private set; }

        protected string SearchTerm { get; set; } = string.Empty;

        private readonly CancellationTokenSource _cts = new();

        protected IEnumerable<Client> FilteredClients =>
            string.IsNullOrWhiteSpace(SearchTerm)
                ? Clients
                : Clients.Where(c =>
                    (c.CompanyName?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.City?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

        protected override async Task OnInitializedAsync()
        {
            await LoadClientsAsync(_cts.Token).ConfigureAwait(false);
        }

        protected async Task LoadClientsAsync(CancellationToken cancellationToken)
        {
            IsLoading = true;
            LoadError = null;

            try
            {
                var list = await RepoClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
                Clients = list.OrderBy(c => c.CompanyName).ToList();
            }
            catch (OperationCanceledException)
            {
                LoadError = "Caricamento annullato.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante il caricamento dei clienti");
                LoadError = "Errore durante il caricamento. Riprova più tardi.";
            }
            finally
            {
                IsLoading = false;
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

        protected void NewClient() => Navigation.NavigateTo("/clients/new");

        protected void Edit(int id) => Navigation.NavigateTo($"/clients/edit/{id}");

        protected void Details(int id) => Navigation.NavigateTo($"/clients/details/{id}");

        protected async Task ConfirmDelete(int id, string? name)
        {
            if (await Js.InvokeAsync<bool>("confirm", $"Eliminare il cliente '{name}'?"))
            {
                try
                {
                    await RepoClient.DeleteAsync(id, _cts.Token).ConfigureAwait(false);
                    await LoadClientsAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    LoadError = "Eliminazione annullata.";
                    await InvokeAsync(() =>
                    {
                        try
                        {
                            StateHasChanged();
                        }
                        catch { }
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Errore durante l'eliminazione del cliente {ClientId}", id);
                    LoadError = $"Errore durante l'eliminazione: {ex.Message}";
                    await InvokeAsync(() =>
                    {
                        try
                        {
                            StateHasChanged();
                        }
                        catch { }
                    });
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
            await Task.CompletedTask;
        }
    }
}