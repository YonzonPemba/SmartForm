using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartForm.ViewModels
{
    public class FormTemplateFieldVM
    {
        public string Guid { get; set; }
        public int SortOrder{ get; set; }
        public string ControlType{ get; set; }
        public string LabelFieldName{ get; set; }

        public string FlexField { get; set; }

        public string FlexFieldValue { get; set; }
    }
}