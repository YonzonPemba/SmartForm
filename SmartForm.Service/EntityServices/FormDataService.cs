using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class FormDataService : GenericService<FormData>
    {
        public FormDataService(IUnitOfWork unitOfWork, IRepository<FormData> repository) : base(unitOfWork, repository)
        {
        }
    }
}
