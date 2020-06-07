using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartForm.ViewModels
{
    public class FormTemplateDetailsVM
    {
        public string  Name { get; set; }
        public string Guid { get; set; }


        public List<FormTemplateFieldVM> FormTemplateFields { get; set; }


    }
}