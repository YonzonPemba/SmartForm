using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class DataTypeService : GenericService<DataType>
    {
        public DataTypeService(IUnitOfWork unitOfWork, IRepository<DataType> repository) : base(unitOfWork, repository)
        {
        }
    }
}
