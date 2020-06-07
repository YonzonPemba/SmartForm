using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class FormService : GenericService<Form>
    {
        public FormService(IUnitOfWork unitOfWork, IRepository<Form> repository) : base(unitOfWork, repository)
        {
        }
    }
}
