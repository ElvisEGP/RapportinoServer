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

namespace RapportinoServer.Pages.Services
{
    public class ListTypeServicePageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected TypeServiceRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime Js { get; set; } = default!;
        [Inject] protected ILogger<ListTypeServicePageBase> Logger { get; set; } = default!;

        protected List<TypeService>? Services { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected bool IsDeleting { get; set; }
        protected string? LoadError { get; set; }

        private CancellationTokenSource? _cts;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync().ConfigureAwait(false);
        }

        private async Task LoadAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsLoading = true;
            LoadError = null;
            Services = null;

            try
            {
                var all = await Repo.GetAllAsync(_cts.Token).ConfigureAwait(false);
                Services = all.ToList();
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore caricamento tipi di servizio");
                LoadError = "Errore durante il caricamento. Riprova più tardi.";
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void NewService() => Nav.NavigateTo("/services/new");

        // Id type changed to int to match repository signatures
        protected void Edit(int id) => Nav.NavigateTo($"/services/edit/{id}");

        protected async Task ConfirmDelete(int id, string? name)
        {
            if (IsDeleting) return;

            try
            {
                var confirmed = await Js.InvokeAsync<bool>("confirm", $"Eliminare il tipo di servizio '{name}'?");
                if (!confirmed) return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Impossibile mostrare conferma eliminazione");
                return;
            }

            await DeleteAsync(id).ConfigureAwait(false);
        }

        private async Task DeleteAsync(int id)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsDeleting = true;

            try
            {
                await Repo.DeleteAsync(id, _cts.Token).ConfigureAwait(false);
                await LoadAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore eliminazione tipo di servizio {TypeServiceId}", id);
            }
            finally
            {
                IsDeleting = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}
