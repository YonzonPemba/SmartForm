using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class FormTemplateFieldService : GenericService<FormTemplateFieldDefinition>
    {
        public FormTemplateFieldService(IUnitOfWork unitOfWork, IRepository<FormTemplateFieldDefinition> repository) : base(unitOfWork, repository)
        {
        }

        public string GetDataType(string controlType)
        {

            switch (controlType.ToLower())
            {
                case "text":
                    return "text";

                case "textarea":
                    return "textcollection";

                default:
                    return "";
            }

        }


    }
}
