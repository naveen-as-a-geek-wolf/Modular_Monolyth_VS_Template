using Shared.DataAccess;

namespace MyCustomApp.Domain.Modules.User
{
    public class User : AuditableEntity
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public User(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
