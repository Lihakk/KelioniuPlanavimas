namespace backend.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

        public bool checkRegistrationData()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && Email.Contains('@')
                && PasswordHash.Length >= 6;
        }

        public bool checkLoginData(string email, string password)
        {
            return AccountStatus == AccountStatus.Active
                && Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                && PasswordHash == password;
        }

        public void blockAccount()
        {
            AccountStatus = AccountStatus.Blocked;
        }

        public void unblockAccount()
        {
            AccountStatus = AccountStatus.Active;
        }
    }
}
