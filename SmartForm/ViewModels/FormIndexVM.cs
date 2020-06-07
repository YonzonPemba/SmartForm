using SmartForm.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartForm.ViewModels
{
    public class FormIndexVM
    {
        public int FormId { get; set; }
        public string FormTemplateName { get; set; }
        public String Status { get; set; }
        public int StatusId { get; set; }
        public String CreatedBy { get; set; }
        public int FormDataId { get; set; }
        //public FormData FormData { get; set; }
        public List<FormTemplateFieldVM> FormTemplateFields { get; set; }

    }
}