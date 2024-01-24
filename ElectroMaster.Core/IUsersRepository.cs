using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Membership;

namespace ElectroMaster.Core
{
    // IUsersRepository.cs
    public interface IUsersRepository
    {
        Task<bool> LoginUser(User user);
        // Other methods...
    }

}
