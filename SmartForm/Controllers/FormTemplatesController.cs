using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using SmartForm.Domain;
using SmartForm.Repository.Repositories;
using SmartForm.Service.EntityServices;
using SmartForm.ViewModels;

namespace SmartForm.Controllers
{
    public class FormTemplatesController : Controller
    {
        private FormTemplateService formTemplateService = new FormTemplateService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<FormTemplate>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));
        private FormService formService = new FormService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<Form>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));
        private  FormTemplateFieldService formTemplateFieldService = new FormTemplateFieldService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<FormTemplateFieldDefinition>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));
        private  DataTypeService dataTypeService = new DataTypeService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<DataType>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));


        public ActionResult _Index() {
            return PartialView("_Index");
        }


        public ActionResult Create(String Guid)
        {
            if (Session["UserName"] != null) {

                List<FormTemplate> formTemplatesDB = formTemplateService.Load(x => x.DateDeleted == null).ToList();
                List<FormTemplateList> formTemplateLists = new List<FormTemplateList>();
                foreach (var formTemplateDB in formTemplatesDB)
                {
                    FormTemplateList formTemplate = new FormTemplateList();
                    formTemplate.Name = formTemplateDB.Name;
                    formTemplate.Guid = formTemplateDB.GUID;
                    formTemplateLists.Add(formTemplate);
                }
                ViewBag.FormTemplateList = formTemplateLists;

                if (Guid != null)
                {
                    FormTemplate formTemplate1 = formTemplateService.Load(x => x.GUID == Guid).FirstOrDefault();
                    FormTemplateDetailsVM formTemplateDetailsVM = new FormTemplateDetailsVM();
                    formTemplateDetailsVM.Name = formTemplate1.Name;
                    formTemplateDetailsVM.Guid = formTemplate1.GUID;

                    formTemplateDetailsVM.FormTemplateFields = GetFormTemplateFieldDefinitions(Guid);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    string jsonData = js.Serialize(formTemplateDetailsVM);
                    ViewBag.Data = jsonData;
                }
                
                return View();
        }
        else{
        TempData["name"] = "You need to be LoggedIn to access this Page";
        return RedirectToAction("Login", "Users");
    }
}



        public String GetData(string guid)
        {
            FormTemplate formTemplateDB = formTemplateService.Load(x => x.DateDeleted == null && x.GUID == guid).FirstOrDefault();
            FormTemplateDetailsVM formTemplateDetailsVM = new FormTemplateDetailsVM();
            formTemplateDetailsVM.Name = formTemplateDB.Name;
            formTemplateDetailsVM.Guid = formTemplateDB.GUID;

            formTemplateDetailsVM.FormTemplateFields = GetFormTemplateFieldDefinitions(guid);

            return (Newtonsoft.Json.JsonConvert.SerializeObject(formTemplateDetailsVM));
        }

        public List<FormTemplateFieldVM> GetFormTemplateFieldDefinitions(String guid)
        {
            FormTemplate formTemplateDB = formTemplateService.Load(x => x.DateDeleted == null && x.GUID == guid).FirstOrDefault();

            List<FormTemplateFieldVM> formTemplateFieldVMs = new List<FormTemplateFieldVM>();
            List<FormTemplateFieldDefinition> formTemplateFieldsDB = formTemplateFieldService.Load(x => x.DateDeleted == null && x.FormTemplateId == formTemplateDB.FormTemplateId).OrderBy(x => x.SortOrder).ToList();
            foreach (var formTemplateFieldDB in formTemplateFieldsDB)
            {
                FormTemplateFieldVM formTemplateFieldVM = new FormTemplateFieldVM();
                formTemplateFieldVM.ControlType = formTemplateFieldDB.ControlType;
                formTemplateFieldVM.Guid = formTemplateFieldDB.GUID;
                formTemplateFieldVM.SortOrder = formTemplateFieldDB.SortOrder;
                formTemplateFieldVM.LabelFieldName = formTemplateFieldDB.LabelFieldName;

                formTemplateFieldVMs.Add(formTemplateFieldVM);
            }
            return formTemplateFieldVMs;
        }


        [HttpPost]
        public ActionResult Create(FormTemplateDetailsVM formTemplate)
        {
            JsonResult jsonResult = new JsonResult();

            if (!formTemplateService.CheckDuplicate(formTemplate.Name,formTemplate.Guid))
            {
                FormTemplate formTemplateDB = formTemplateService.Load(x => x.DateDeleted == null && x.GUID == formTemplate.Guid).FirstOrDefault();
                if (formTemplateDB == null) {
                    FormTemplate formTemplateNew = new FormTemplate();
                    formTemplateNew.Name = formTemplate.Name;
                    formTemplateNew.GUID = formTemplate.Guid;
                    formTemplateNew.DateCreated = DateTime.Now;
                    formTemplateService.Add(formTemplateNew);
                    formTemplateService.Save();

                    int newFormTemplateId = formTemplateNew.FormTemplateId;
                    int labelFieldCounter = 0;
                    foreach (var item in formTemplate.FormTemplateFields)
                    {
                        labelFieldCounter++;
                        AddFormTemplateField(item, newFormTemplateId, formTemplateNew.DateCreated,labelFieldCounter);
                    }

                    formTemplateFieldService.Save();
                    jsonResult.Data = new { Success = "true", Message = "FormTemplate Created successfully" };
                    return jsonResult;
                }
                else
                 {
                    DateTime currentDate = DateTime.Now;

                    int formTemplateId = formTemplateService.Load(x => x.DateDeleted == null && x.GUID == formTemplate.Guid).Select(x => x.FormTemplateId).FirstOrDefault();
                    formTemplateDB.Name = formTemplate.Name;
                    formTemplateDB.DateModfied = currentDate;
                    //formTemplateService.Update(formTemplateDB);
                    formTemplateService.Save();

                    List<int> existingIds = formTemplateFieldService.Load(x => x.DateDeleted == null && x.FormTemplateId == formTemplateId).Select(x => x.FormTemplateFieldDefinitionId).ToList();
                    List<string> guids = formTemplate.FormTemplateFields.Select(x => x.Guid).ToList();
                    List<int> updatedIds = formTemplateFieldService.Load(x => guids.Contains(x.GUID)).Select(x => x.FormTemplateFieldDefinitionId).ToList();
                    List<int> deletedIds = existingIds.Except(updatedIds).ToList();


                    int labelFieldCounter = formTemplateFieldService.Load(x=>x.FormTemplateId==formTemplateId).Count();

                    foreach (var item in formTemplate.FormTemplateFields)
                    {
                        FormTemplateFieldDefinition formTemplateFieldDefinitionDB = formTemplateFieldService.Load(x => x.DateDeleted == null && x.GUID == item.Guid).FirstOrDefault();
                        if (formTemplateFieldDefinitionDB != null)
                        {
                            formTemplateFieldDefinitionDB.LabelFieldName = item.LabelFieldName;
                            formTemplateFieldDefinitionDB.SortOrder = item.SortOrder;
                            formTemplateFieldDefinitionDB.DateModfied = currentDate;
                            //formTemplateFieldService.Update(formTemplateFieldDefinitionDB);
                        }
                        else
                        {
                            labelFieldCounter++;
                            AddFormTemplateField(item, formTemplateId, currentDate,labelFieldCounter);
                        }
                    }

                    foreach (var item in deletedIds)
                    {
                        FormTemplateFieldDefinition formTemplateFieldDefinitionDB = formTemplateFieldService.Load(x => x.DateDeleted == null && x.FormTemplateFieldDefinitionId == item).FirstOrDefault();
                        formTemplateFieldDefinitionDB.DateDeleted = currentDate;
                    }

                    formTemplateFieldService.Save();
                    jsonResult.Data = new { Success = "true", Message = "FormTemplate updated Successfully" };
                    return jsonResult;
                }
            }
            else 
            {
                jsonResult.Data = new { Success = "false", Message = "FormTemplate with given name already exists" };
                return jsonResult;
            }
        }



        [HttpPost]
        public ActionResult Delete(string guid) {

            FormTemplate formTemplate = formTemplateService.Load(x => x.GUID == guid && x.DateDeleted==null).FirstOrDefault();
            JsonResult jsonResult = new JsonResult();
            if (formTemplate!=null)
            {
                DateTime dateTime = DateTime.Now;
                int id = formTemplate.FormTemplateId;
                formTemplate.DateDeleted = dateTime;
                List<Form> forms = formService.Load(x => x.FormTemplateId == id && x.DateDeleted == null).ToList();
                foreach (var item in forms) {
                    item.DateDeleted = dateTime;
                }
                formTemplateService.Save();
                formService.Save();
                jsonResult.Data = new { Success = "true", Message = "FormTemplate was deleted successfully" };
                return jsonResult;
            }
            jsonResult.Data = new { Success = "false", Message = "Error in Deleting FormTemplate" };
            return jsonResult;
        }


        private void AddFormTemplateField(FormTemplateFieldVM item, int formTemplateId,DateTime dateTime,int labelFieldCounter)
        {
            var dataTypeName = formTemplateFieldService.GetDataType(item.ControlType);

            FormTemplateFieldDefinition formTemplateFieldDefinition = new FormTemplateFieldDefinition();
            formTemplateFieldDefinition.FormTemplateId = formTemplateId;
            formTemplateFieldDefinition.GUID = item.Guid;
            formTemplateFieldDefinition.ControlType = item.ControlType;
            formTemplateFieldDefinition.LabelFieldName = item.LabelFieldName;
            formTemplateFieldDefinition.SortOrder = item.SortOrder;
            formTemplateFieldDefinition.FieldName = "FlexField" + labelFieldCounter;
            formTemplateFieldDefinition.DateCreated = dateTime;
            formTemplateFieldDefinition.DataTypeId = dataTypeService.Load(x => x.Name == dataTypeName).Select(x => x.DataTypeId).FirstOrDefault();
            formTemplateFieldService.Add(formTemplateFieldDefinition);

        }

    }
}
