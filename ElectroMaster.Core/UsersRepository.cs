using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace ElectroMaster.Core
{
    // UsersRepository.cs
    public class UsersRepository : IUsersRepository
    {
        private readonly List<BackOfficeIdentityUser> _validUsers = new List<BackOfficeIdentityUser>
        {
            new BackOfficeIdentityUser(
                // Provide required parameters for the constructor
                new GlobalSettings(),
                0, // You may need to adjust the integer value based on your scenario
                Enumerable.Empty<IReadOnlyUserGroup>()
            )
            {
                UserName = "user1",
                PasswordHash = "password1"
            },
            new BackOfficeIdentityUser(
                // Provide required parameters for the constructor
                new GlobalSettings(),
                0, // You may need to adjust the integer value based on your scenario
                Enumerable.Empty<IReadOnlyUserGroup>()
            )
            {
                UserName = "user2",
                PasswordHash = "password2"
            },
            // Add more users as needed
        };

        public Task<bool> LoginUser(BackOfficeIdentityUser user)
        {
            // Your logic for user authentication
            // Return true if the user is valid, otherwise false
            // This could involve checking credentials against a database, for example.

            // Example: Check if the provided user credentials match any valid user
            var validUser = _validUsers.FirstOrDefault(u =>
                u.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash.Equals(user.PasswordHash));

            bool isUserValid = validUser != null;

            return Task.FromResult(isUserValid);
        }

        public Task<bool> LoginUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
