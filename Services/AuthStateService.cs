using RapportinoServer.Models;

namespace RapportinoServer.Services
{
    public class AuthStateService
    {
        private static readonly HashSet<string> NonAdminAllowedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/home",
            "/clients/new",
            "/reports",
            "/reports/new"
        };

        public Technician? CurrentTechnician { get; private set; }

        public bool IsAuthenticated => CurrentTechnician is not null;
        public bool IsAdmin =>
            string.Equals(CurrentTechnician?.Username, "admin", StringComparison.OrdinalIgnoreCase);

        public event Action? OnAuthStateChanged;

        public void SignIn(Technician technician)
        {
            CurrentTechnician = technician;
            OnAuthStateChanged?.Invoke();
        }

        public void SignOut()
        {
            CurrentTechnician = null;
            OnAuthStateChanged?.Invoke();
        }

        public bool CanAccessPath(string path)
        {
            if (!IsAuthenticated)
                return false;

            if (IsAdmin)
                return true;

            var normalized = NormalizePath(path);
            if (NonAdminAllowedPaths.Contains(normalized))
                return true;

            // Dettaglio rapportino dalla lista / dashboard
            if (normalized.StartsWith("/reports/details/", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "/";

            var normalized = "/" + path.Trim().Trim('/').ToLowerInvariant();
            return normalized == "//" ? "/" : normalized;
        }
    }
}
