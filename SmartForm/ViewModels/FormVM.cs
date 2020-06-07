using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartForm.ViewModels
{
    public class FormVM
    {
        public int FormTemplateId { get; set; }

        public List<FormDataVM> FormDataVMs { get; set; }
    }
}