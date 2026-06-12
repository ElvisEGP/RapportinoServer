using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Clients
{
    public class ClientDeletePageBase : ComponentBase, IAsyncDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected ClientRepository RepoClient { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<ClientDeletePageBase> Logger { get; set; } = default!;

        protected Client? Client { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected bool IsDeleting { get; set; }
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
            Client = null;

            try
            {
                Client = await RepoClient.GetByIdAsync(Id, _cts.Token);

                if (Client is null)
                    LoadError = null; // deixa o markup mostrar "Cliente non trovato"
            }
            catch (OperationCanceledException)
            {
                // ignorado
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao carregar cliente {ClientId}", Id);
                LoadError = "Errore durante il caricamento. Riprova più tardi.";
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task ConfirmDelete()
        {
            if (Client is null)
            {
                ErrorMessage = "Cliente non disponibile per l'eliminazione.";
                return;
            }

            IsDeleting = true;
            ErrorMessage = null;

            try
            {
                await RepoClient.DeleteAsync(Id, CancellationToken.None);
                Nav.NavigateTo("/clients");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
                Logger.LogInformation("Eliminazione cliente {ClientId} annullata.", Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante l'eliminazione del cliente {ClientId}", Id);
                ErrorMessage = "Errore durante l'eliminazione. Verifica che non ci siano dipendenze (es. macchine collegate).";
            }
            finally
            {
                IsDeleting = false;
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
