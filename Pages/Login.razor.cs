using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Services;

namespace RapportinoServer.Pages
{
    public class LoginPageBase : ComponentBase
    {
        [Inject] protected TechnicianRepository TechnicianRepository { get; set; } = default!;
        [Inject] protected AuthStateService AuthState { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected LoginFormModel Form { get; private set; } = new();
        protected EditContext EditContext { get; private set; } = default!;
        protected bool IsSubmitting { get; private set; }
        protected string? ErrorMessage { get; private set; }

        protected override void OnInitialized()
        {
            EditContext = new EditContext(Form);

            if (AuthState.IsAuthenticated)
            {
                Nav.NavigateTo("/home");
            }
        }

        protected async Task HandleValidSubmit()
        {
            if (IsSubmitting)
                return;

            IsSubmitting = true;
            ErrorMessage = null;

            try
            {
                var technician = await TechnicianRepository.ValidateCredentialsAsync(Form.Username, Form.Password).ConfigureAwait(false);
                if (technician is null)
                {
                    ErrorMessage = "Username o password non validi.";
                    return;
                }

                AuthState.SignIn(technician);
                var returnUrl = GetReturnUrl();
                Nav.NavigateTo(returnUrl);
            }
            catch (Exception)
            {
                ErrorMessage = "Errore durante il login.";
            }
            finally
            {
                IsSubmitting = false;
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

        private string GetReturnUrl()
        {
            var uri = Nav.ToAbsoluteUri(Nav.Uri);
            if (string.IsNullOrWhiteSpace(uri.Query))
                return "/home";

            var query = QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("returnUrl", out var returnUrlValue))
            {
                var returnUrl = returnUrlValue.ToString();
                if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/'))
                    return returnUrl;
            }

            return "/home";
        }

        protected class LoginFormModel
        {
            [Required(ErrorMessage = "Inserire lo username")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Inserire la password")]
            public string Password { get; set; } = string.Empty;
        }
    }
}
