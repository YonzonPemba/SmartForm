using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class FormTemplateService : GenericService<FormTemplate>
    {
        public FormTemplateService(IUnitOfWork unitOfWork, IRepository<FormTemplate> repository) : base(unitOfWork, repository)
        {
        }

        public bool CheckDuplicate(string formTemplateName,string Guid)
        {

            List<FormTemplate> formTemplates = repository.Load(x => x.DateDeleted == null && x.Name.ToLower() == formTemplateName.ToLower()).ToList();
            FormTemplate formTemplate = repository.Load(x => x.GUID == Guid).FirstOrDefault();
            if (formTemplate == null) {
                formTemplate = new FormTemplate();
            }


            formTemplates.RemoveAll(x => x.GUID == Convert.ToString(formTemplate.GUID));
            if (formTemplates.Count >= 1) {
                return true;
            }
            return false;
        }

        
    }
}

