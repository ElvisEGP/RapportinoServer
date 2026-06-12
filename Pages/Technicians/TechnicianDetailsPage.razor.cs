using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Technicians
{
    public class TechnicianDetailsPageBase : ComponentBase
    {
        [Parameter] public int Id { get; set; }

        [Inject] protected TechnicianRepository Repo { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        protected Technician? Technician { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected string? LoadError { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await LoadTechnicianAsync(CancellationToken.None);
        }

        private async Task LoadTechnicianAsync(CancellationToken cancellationToken)
        {
            IsLoading = true;
            LoadError = null;

            try
            {
                Technician = await Repo.GetByIdAsync(Id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LoadError = "Errore durante il caricamento del tecnico: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void BackToList() => Navigation.NavigateTo("/technicians");
    }
}
