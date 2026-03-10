namespace AuthService.Services
{
    public static class PasswordService
    {
        // Nombre de tours de clé du coffre fort
        // Plus c'est haut, plus c'est difficile et long à vérifier (pour nous et pour les pirates)
        private const int WorkFactor = 11;

        // Hash un mot de passe en clair
        // A utiliser lors de l'inscription ou du changement de mot de passe
        public static string HashPassword(string password)
        {
            // Génère automatiquement un salt aléatoire
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        // Vérifie si un mot de passe correspond à un hash
        public static bool VerifyPassword(string password, string passwordHash)
        {
            // Retourne true si les hash correspondent
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
