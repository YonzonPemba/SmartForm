using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class FormStatusService : GenericService<FormStatu>
    {
        public FormStatusService(IUnitOfWork unitOfWork, IRepository<FormStatu> repository) : base(unitOfWork, repository)
        {
        }
    }
}
