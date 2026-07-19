using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Models;

namespace RapportinoServer.Pages.Reports
{
    public class NewReportPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected ReportRepository RepoReport { get; set; } = default!;
        [Inject] protected ClientRepository RepoClient { get; set; } = default!;
        [Inject] protected MachineRepository RepoMachine { get; set; } = default!;
        [Inject] protected TechnicianRepository RepoTechnician { get; set; } = default!;
        [Inject] protected TypeServiceRepository RepoService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected ILogger<NewReportPageBase> Logger { get; set; } = default!;
        
        [Inject] protected IJSRuntime Js { get; set; } = default!;

        protected Report Report { get; set; } = new();
        protected EditContext EditContext { get; set; } = default!;
        protected bool IsSaving { get; set; }
        protected string? SaveError { get; set; }

        protected string ClientSearch { get; set; } = string.Empty;
        protected bool ShowClientList { get; set; }
        protected List<Client> Clients { get; set; } = new();
        protected Client? SelectedClient { get; set; }

        protected List<Machine> Machines { get; set; } = new();
        protected Machine? SelectedMachine { get; set; }
        protected int? SelectedMachineId { get; set; }
        protected string MachineRapporto { get; set; } = string.Empty;

        protected string SelectedSerial { get; set; } = string.Empty;

        protected List<int> logHours { get; set; } = new();
        protected List<int> logMinutes { get; set; } = new();

        protected List<Technician> Technicians { get; set; } = new();
        protected List<TypeService> Services { get; set; } = new();

        protected IEnumerable<Client> FilteredClients =>
            !ShowClientList
                ? Enumerable.Empty<Client>()
                : string.IsNullOrWhiteSpace(ClientSearch)
                    ? Clients.Take(10)
                    : Clients
                        .Where(c =>
                            (c.CompanyName?.Contains(ClientSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (c.City?.Contains(ClientSearch, StringComparison.OrdinalIgnoreCase) ?? false))
                        .Take(10);

        protected IEnumerable<Machine> FilteredMachines =>
            SelectedClient is null
                ? Enumerable.Empty<Machine>()
                : Machines.Where(m => m.ClientId == SelectedClient.Id);

        protected IEnumerable<string> FilteredSerials =>
            SelectedClient is null
                ? Enumerable.Empty<string>()
                : Machines
                    .Where(m => m.ClientId == SelectedClient.Id)
                    .Select(m => m.Serial)
                    .Distinct()
                    .OrderBy(s => s);

        private CancellationTokenSource? _cts;

        protected override async Task OnInitializedAsync()
        {
            Report = CreateEmptyReport();
            EditContext = new EditContext(Report);
            EnsureLogHelpers();

            await LoadDataAsync().ConfigureAwait(false);
        }

        private static Report CreateEmptyReport()
        {
            return new Report
            {
                Data = DateOnly.FromDateTime(DateTime.Today),
                ReportDate = DateOnly.FromDateTime(DateTime.Today),
                Serial = string.Empty,
                Model = string.Empty,
                ReportDescription = string.Empty,
                TechnicianName = string.Empty,
                ServiceType = string.Empty,
                Email = string.Empty,
                SafetyDevicesCheck = true,
                WorkLogs = new List<ReportWorkLog>
                {
                    new ReportWorkLog
                    {
                        WorkedDate = DateOnly.FromDateTime(DateTime.Today),
                        WorkedTime = TimeSpan.Zero
                    }
                }
            };
        }

        protected void NewClient()
        {
            Nav.NavigateTo("/clients/new");
        }

        private async Task LoadDataAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            SaveError = null;

            await LoadClientsAsync(_cts.Token).ConfigureAwait(false);
            await LoadMachinesAsync(_cts.Token).ConfigureAwait(false);
            await LoadTechniciansAsync(_cts.Token).ConfigureAwait(false);
            await LoadServicesAsync(_cts.Token).ConfigureAwait(false);

            EnsureLogHelpers();

            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadClientsAsync(CancellationToken cancellationToken)
        {
            try
            {
                Clients = (await RepoClient.GetAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError(ex, "Errore caricamento clienti");
                SaveError = "Errore durante il caricamento dei clienti.";
            }
        }

        private async Task LoadMachinesAsync(CancellationToken cancellationToken)
        {
            try
            {
                Machines = (await RepoMachine.GetAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError(ex, "Errore caricamento macchine");
                SaveError = AppendError(SaveError, "Errore durante il caricamento delle macchine.");
            }
        }

        private async Task LoadTechniciansAsync(CancellationToken cancellationToken)
        {
            try
            {
                Technicians = (await RepoTechnician.GetAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError(ex, "Errore caricamento tecnici");
                SaveError = AppendError(SaveError, "Errore durante il caricamento dei tecnici.");
            }
        }

        private async Task LoadServicesAsync(CancellationToken cancellationToken)
        {
            try
            {
                Services = (await RepoService.GetAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError(ex, "Errore caricamento servizi");
                SaveError = AppendError(SaveError, "Errore durante il caricamento dei servizi.");
            }
        }

        private static string AppendError(string? current, string message)
        {
            return string.IsNullOrWhiteSpace(current)
                ? message
                : $"{current} {message}";
        }

        protected void ShowClientsList()
        {
            ShowClientList = true;
        }

        protected void HideClientsList()
        {
            ShowClientList = false;
        }

        protected void OnClientSearchInput(ChangeEventArgs e)
        {
            ClientSearch = e.Value?.ToString() ?? string.Empty;
            ShowClientList = true;

            if (SelectedClient is not null &&
                !string.Equals(ClientSearch, SelectedClient.CompanyName, StringComparison.OrdinalIgnoreCase))
            {
                SelectedClient = null;
                Report.ClientId = 0;
                Report.Email = string.Empty;
                SelectedMachine = null;
                SelectedMachineId = null;
                SelectedSerial = string.Empty;
                Report.MachineId = null;
                Report.Model = string.Empty;
                Report.Serial = string.Empty;
                MachineRapporto = string.Empty;
            }
        }

        protected void SelectClient(Client client)
        {
            SelectedClient = client;
            ClientSearch = client.CompanyName ?? string.Empty;
            ShowClientList = false;

            Report.ClientId = client.Id;
            Report.Email = client.Email ?? string.Empty;

            SelectedMachine = null;
            SelectedMachineId = null;
            Report.MachineId = null;
            Report.Model = string.Empty;
            Report.Serial = string.Empty;
            SelectedSerial = string.Empty;
            MachineRapporto = string.Empty;
        }

        protected void OnSerialSelected()
        {
            if (string.IsNullOrWhiteSpace(SelectedSerial))
            {
                Report.Serial = string.Empty;
                Report.Model = string.Empty;
                return;
            }

            // Busca a máquina com esse serial
            var machine = Machines.FirstOrDefault(m => 
                m.ClientId == SelectedClient?.Id && 
                m.Serial == SelectedSerial);

            if (machine is not null)
            {
                Report.Serial = machine.Serial ?? string.Empty;
                Report.Model = machine.Model ?? string.Empty;
            }
        }

        protected async Task OnMachineSelected()
        {
            if (SelectedMachineId is null)
            {
                SelectedMachine = null;
                Report.MachineId = null;
                Report.Serial = string.Empty;
                Report.Model = string.Empty;
                return;
            }

            SelectedMachine = Machines.FirstOrDefault(m => m.Id == SelectedMachineId.Value);

            if (SelectedMachine is not null)
            {
                Report.MachineId = SelectedMachine.Id;
                Report.Serial = SelectedMachine.Serial ?? string.Empty;
                Report.Model = SelectedMachine.Model ?? string.Empty;
            }

            await Task.CompletedTask;
        }

        protected void AddLog()
        {
            Report.WorkLogs.Add(new ReportWorkLog
            {
                WorkedDate = DateOnly.FromDateTime(DateTime.Today),
                WorkedTime = TimeSpan.Zero
            });

            EnsureLogHelpers();
        }

        protected void RemoveLogAt(int index)
        {
            if (index < 0 || index >= Report.WorkLogs.Count)
                return;

            Report.WorkLogs.RemoveAt(index);
            EnsureLogHelpers();
        }

        protected void SetLogHour(int index, string? value)
        {
            if (index < 0 || index >= logHours.Count)
                return;

            if (int.TryParse(value, out var hour))
            {
                logHours[index] = hour;
                UpdateWorkLogTime(index);
                StateHasChanged();
            }
        }

        protected void SetLogMinute(int index, string? value)
        {
            if (index < 0 || index >= logMinutes.Count)
                return;

            if (int.TryParse(value, out var minute))
            {
                logMinutes[index] = minute;
                UpdateWorkLogTime(index);
                StateHasChanged();
            }
        }

        private void UpdateWorkLogTime(int index)
        {
            if (index < 0 || index >= Report.WorkLogs.Count)
                return;

            Report.WorkLogs[index].WorkedTime = new TimeSpan(logHours[index], logMinutes[index], 0);
        }

        private void EnsureLogHelpers()
        {
            while (logHours.Count < Report.WorkLogs.Count)
                logHours.Add(0);

            while (logMinutes.Count < Report.WorkLogs.Count)
                logMinutes.Add(0);

            if (logHours.Count > Report.WorkLogs.Count)
                logHours.RemoveRange(Report.WorkLogs.Count, logHours.Count - Report.WorkLogs.Count);

            if (logMinutes.Count > Report.WorkLogs.Count)
                logMinutes.RemoveRange(Report.WorkLogs.Count, logMinutes.Count - Report.WorkLogs.Count);

            for (var i = 0; i < Report.WorkLogs.Count; i++)
            {
                logHours[i] = Report.WorkLogs[i].WorkedTime.Hours;
                logMinutes[i] = Report.WorkLogs[i].WorkedTime.Minutes;
            }
        }

        protected async Task HandleValidSubmit()
        {
            if (IsSaving)
                return;

            IsSaving = true;
            SaveError = null;

            using var saveCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            try
            {
                // Atualiza horas/minutos
                for (var i = 0; i < Report.WorkLogs.Count; i++)
                    UpdateWorkLogTime(i);

                await RepoReport.InsertAsync(Report, saveCts.Token).ConfigureAwait(true);

                Nav.NavigateTo("/reports");
            }
            catch (OperationCanceledException)
            {
                SaveError = "Operazione annullata.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Errore salvataggio nuovo rapportino");
                SaveError = "Errore durante il salvataggio.";
            }
            finally
            {
                IsSaving = false;
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


        protected async Task ResetForm()
        {
            Report = CreateEmptyReport();
            EditContext = new EditContext(Report);
            ClientSearch = string.Empty;
            ShowClientList = false;
            SelectedClient = null;
            SelectedMachine = null;
            SelectedMachineId = null;
            MachineRapporto = string.Empty;
            SaveError = null;
            logHours.Clear();
            logMinutes.Clear();
            EnsureLogHelpers();
            await Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}