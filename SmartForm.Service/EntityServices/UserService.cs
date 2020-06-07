using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class UserService:GenericService<User>
    {
        public UserService(IUnitOfWork unitOfWork, IRepository<User> repository) : base(unitOfWork, repository)
        {
        }
    }
}
