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

namespace RapportinoServer.Pages.Machines
{
    public class MachineListPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected MachineRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime Js { get; set; } = default!;
        [Inject] protected ILogger<MachineListPageBase> Logger { get; set; } = default!;

        protected List<Machine>? Machines { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected bool IsDeleting { get; set; }
        protected string? LoadError { get; set; }

        protected string MachineSearch { get; set; } = string.Empty;

        private CancellationTokenSource? _cts;

        protected IEnumerable<Machine> FilteredMachines =>
            string.IsNullOrWhiteSpace(MachineSearch)
                ? (Machines ?? Enumerable.Empty<Machine>())
                : (Machines ?? Enumerable.Empty<Machine>()).Where(m =>
                    (m.Model?.Contains(MachineSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (m.Serial?.Contains(MachineSearch, StringComparison.OrdinalIgnoreCase) ?? false));

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
            Machines = null;

            try
            {
                var all = await Repo.GetAllAsync(_cts.Token).ConfigureAwait(false);
                Machines = all.ToList();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore caricamento macchine");
                LoadError = "Errore durante il caricamento. Riprova più tardi.";
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void NewMachine() => Nav.NavigateTo("/machines/new");

        protected void Edit(int id) => Nav.NavigateTo($"/machines/edit/{id}");

        protected void Details(int id) => Nav.NavigateTo($"/machines/details/{id}");

        protected async Task ConfirmDelete(int id, string? name)
        {
            if (IsDeleting) return;

            try
            {
                var confirmed = await Js.InvokeAsync<bool>("confirm", $"Eliminare la macchina '{name}'?");
                if (!confirmed) return;

                IsDeleting = true;

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                await Repo.DeleteAsync(id, _cts.Token).ConfigureAwait(false);

                var all = await Repo.GetAllAsync(_cts.Token).ConfigureAwait(false);
                Machines = all.ToList();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore eliminazione macchina {MachineId}", id);
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