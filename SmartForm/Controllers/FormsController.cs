using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using SmartForm.Domain;
using SmartForm.Repository.Repositories;
using SmartForm.Service.EntityServices;
using SmartForm.ViewModels;

namespace SmartForm.Controllers
{
    public class FormsController : Controller
    {
        private SmartFormEntities db = new SmartFormEntities();
        private FormService formService = new FormService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<Form>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));
        private FormStatusService formStatusService = new FormStatusService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<FormStatu>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));
        private FormDataService formDataService = new FormDataService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<FormData>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));
        private FormTemplateService formTemplateService = new FormTemplateService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<FormTemplate>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));
        private FormTemplateFieldService formTemplateFieldService = new FormTemplateFieldService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<FormTemplateFieldDefinition>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));

        public ActionResult Index()
        {
            if (Session["UserName"] != null) { 
            List<FormIndexVM> FormIndexVMs = new List<FormIndexVM>();
            List<Form> forms = formService.Load(x => x.DateDeleted == null).OrderByDescending(x=>x.DateCreated).ToList();

            foreach (var item in forms)
            {
                FormIndexVM formIndexVM = new FormIndexVM();
                formIndexVM.FormId = item.FormId;
                formIndexVM.FormTemplateName = item.FormTemplate.Name;
                formIndexVM.Status = item.FormStatu.FormStatus;
                formIndexVM.CreatedBy = item.User.UserName;
                formIndexVM.FormDataId = item.FormDatas.FirstOrDefault().FormDataId;
                FormData currentFormData = item.FormDatas.FirstOrDefault();

                var formTemplateFieldDefinitions = formTemplateFieldService.Load(x => x.DateDeleted == null && x.FormTemplateId == item.FormTemplateId).OrderBy(x=>x.SortOrder).ToList();
                List<FormTemplateFieldVM> formTemplateFields = new List<FormTemplateFieldVM>();
                foreach (var formTemplateField in formTemplateFieldDefinitions)
                {
                    FormTemplateFieldVM formTemplateFieldVM = new FormTemplateFieldVM();

                    formTemplateFieldVM.ControlType = formTemplateField.ControlType;
                    formTemplateFieldVM.LabelFieldName = formTemplateField.LabelFieldName;
                    formTemplateFieldVM.SortOrder = formTemplateField.SortOrder;
                    formTemplateFieldVM.FlexField = formTemplateField.FieldName;
                    String fieldname = formTemplateField.FieldName;
                    String Value = Convert.ToString(currentFormData[fieldname]);
                    formTemplateFieldVM.FlexFieldValue = Value;
                    formTemplateFields.Add(formTemplateFieldVM);
                }
                formIndexVM.FormTemplateFields = formTemplateFields;

                FormIndexVMs.Add(formIndexVM);
            }

            var datas = JsonConvert.SerializeObject(FormIndexVMs);
            ViewBag.Datas = datas;
            //var array = FormIndexVMs.Select(x => x.FormTemplateName).Distinct();
                var array = formTemplateService.Load(x => x.DateDeleted == null).Select(x => x.Name).ToList();
            ViewBag.UniqueTemplates = JsonConvert.SerializeObject(array);
            return View(FormIndexVMs);
        }
        else{
        TempData["name"] = "You need to be LoggedIn to access this Page";
        return RedirectToAction("Login", "Users");
    }

}

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Form form = db.Forms.Find(id);
            if (form == null)
            {
                return HttpNotFound();
            }
            return View(form);
        }

        public ActionResult Create()
        {
            //ViewBag.FormTemplateId = new SelectList(formTemplateService.Load(x=>x.DateDeleted==null).ToList(), "FormTemplateId", "Name");
            //return View();
            if (Session["UserName"]!=null) { 
            List<FormTemplate> formTemplates = formTemplateService.Load(x => x.DateDeleted == null).ToList();
            return View(formTemplates);
        }
        else{
        TempData["name"] = "You need to be LoggedIn to access this Page";
        return RedirectToAction("Login", "Users");
    }
}


        public string GetFormFields(int id) {

            var formTemplateFieldDefinitions = formTemplateFieldService.Load(x => x.DateDeleted == null&&x.FormTemplateId==id).ToList();
            List<FormTemplateFieldVM> formTemplateFields = new List<FormTemplateFieldVM>();

            foreach (var item in formTemplateFieldDefinitions) {

                FormTemplateFieldVM formTemplateField = new FormTemplateFieldVM();
                formTemplateField.ControlType = item.ControlType;
                formTemplateField.LabelFieldName = item.LabelFieldName;
                formTemplateField.SortOrder = item.SortOrder;
                formTemplateField.Guid = item.GUID;
                formTemplateField.FlexField = item.FieldName;
                formTemplateFields.Add(formTemplateField);
            }

            return (JsonConvert.SerializeObject(formTemplateFields));
        }



        [HttpPost]
        public JsonResult Create(FormVM formVM) {

            JsonResult jsonResult = new JsonResult();
            if (formVM != null)
            {
                Form form = new Form();
                form.GUID = Convert.ToString(Guid.NewGuid());
                form.UserId = 1;
                form.FormTemplateId = formVM.FormTemplateId;
                form.FormStatus = 4;
                form.DateCreated = DateTime.Now;
                formService.Add(form);
                formService.Save();

                int formId = form.FormId;

                FormData formData = new FormData();
                formData.GUID = Convert.ToString(Guid.NewGuid());
                formData.FormId = formId;
                formData.DateCreated = form.DateCreated;

                foreach (var item in formVM.FormDataVMs)
                {
                    String flexFieldValue = item.FlexField;
                    formData[flexFieldValue] = item.Value;
                }
                formDataService.Add(formData);
                formDataService.Save();

                jsonResult.Data = new { Sucess = "True", Message = "FormData added successfully" };
            }
            else {
                jsonResult.Data = new { Sucess = "False", Message = "FormData could not be added" };
            }
            return jsonResult;
        }



        public ActionResult Edit(int? id)
        {
            if (Session["UserName"]!=null) { 

            int SelectedStatus = formService.LoadByID(id).FormStatus;
            ViewBag.StatusId = new SelectList(formStatusService.Load(x => x.DateDeleted == null).ToList(), "FormStatusId", "FormStatus",SelectedStatus);

            Form form = formService.Load(x => x.DateDeleted == null && x.FormId == id).FirstOrDefault();

            FormIndexVM formIndexVM = new FormIndexVM();
            formIndexVM.FormId = form.FormId;
            formIndexVM.FormTemplateName = form.FormTemplate.Name;
            formIndexVM.Status = form.FormStatu.FormStatus;
            formIndexVM.CreatedBy = form.User.UserName;
            formIndexVM.FormDataId = form.FormDatas.FirstOrDefault().FormDataId;
            FormData currentFormData = form.FormDatas.FirstOrDefault();

            var formTemplateFieldDefinitions = formTemplateFieldService.Load(x => x.DateDeleted == null && x.FormTemplateId == form.FormTemplateId).ToList();
            List<FormTemplateFieldVM> formTemplateFields = new List<FormTemplateFieldVM>();
            foreach (var formTemplateField in formTemplateFieldDefinitions)
            {
                FormTemplateFieldVM formTemplateFieldVM = new FormTemplateFieldVM();

                formTemplateFieldVM.ControlType = formTemplateField.ControlType;
                formTemplateFieldVM.LabelFieldName = formTemplateField.LabelFieldName;
                formTemplateFieldVM.SortOrder = formTemplateField.SortOrder;
                formTemplateFieldVM.FlexField = formTemplateField.FieldName;
                String fieldname = formTemplateField.FieldName;
                String Value = Convert.ToString(currentFormData[fieldname]);
                formTemplateFieldVM.FlexFieldValue = Value;
                formTemplateFields.Add(formTemplateFieldVM);
            }
            formIndexVM.FormTemplateFields = formTemplateFields;

            var datas = JsonConvert.SerializeObject(formIndexVM);
            ViewBag.Datas = datas;

            return View();
        }
        else{
        TempData["name"] = "You need to be LoggedIn to access this Page";
        return RedirectToAction("Login", "Users");
    }
}





        [HttpPost]
        public JsonResult Edit(FormIndexVM formIndexVM)
        {
            JsonResult jsonResult = new JsonResult();
            Form form = formService.LoadByID(formIndexVM.FormId);
            FormData formData = formDataService.LoadByID(formIndexVM.FormDataId);

            form.DateModfied = DateTime.Now;
            form.FormStatus = formIndexVM.StatusId;

            foreach (var item in formIndexVM.FormTemplateFields) {
                String flexField = item.FlexField;
                formData[flexField] = item.FlexFieldValue;
            }
            formData.DateModfied = form.DateModfied;

            formService.Save();
            formDataService.Save();

            jsonResult.Data = new { Success = "true", Message = "Form was Updated Successfully" };

            return jsonResult;
        }

        // GET: Forms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Form form = db.Forms.Find(id);
            if (form == null)
            {
                return HttpNotFound();
            }
            return View(form);
        }

        // POST: Forms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Form form = db.Forms.Find(id);
            db.Forms.Remove(form);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
