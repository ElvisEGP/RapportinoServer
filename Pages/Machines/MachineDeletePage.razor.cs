using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Machines
{
    public class MachineDeletePageBase : ComponentBase, IAsyncDisposable
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected MachineRepository RepoMachine { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected ILogger<MachineDeletePageBase> Logger { get; set; } = default!;

        protected Machine? Machine { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected bool IsDeleting { get; set; }
        protected string? ErrorMessage { get; set; }
        protected string? LoadError { get; set; }

        private CancellationTokenSource? _cts;

        protected override async Task OnParametersSetAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            IsLoading = true;
            LoadError = null;
            ErrorMessage = null;
            Machine = null;

            try
            {
                Machine = await RepoMachine.GetByIdAsync(Id, _cts.Token);

                if (Machine is null)
                    LoadError = null;
            }
            catch (OperationCanceledException)
            {
                // ignorado
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao carregar máquina {MachineId}", Id);
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
            if (Machine is null)
            {
                ErrorMessage = "Macchina non disponibile per l'eliminazione.";
                return;
            }

            IsDeleting = true;
            ErrorMessage = null;

            try
            {
                await RepoMachine.DeleteAsync(Id, _cts?.Token ?? CancellationToken.None);
                Navigation.NavigateTo("/machines");
            }
            catch (OperationCanceledException)
            {
                ErrorMessage = "Operazione annullata.";
                Logger.LogInformation("Eliminazione macchina {MachineId} annullata.", Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore durante l'eliminazione della macchina {MachineId}", Id);
                ErrorMessage = "Errore durante l'eliminazione.";
            }
            finally
            {
                IsDeleting = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void Cancel() => Navigation.NavigateTo("/machines");

        public async ValueTask DisposeAsync()
        {
            if (_cts is not null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            await Task.CompletedTask;
        }
    }
}