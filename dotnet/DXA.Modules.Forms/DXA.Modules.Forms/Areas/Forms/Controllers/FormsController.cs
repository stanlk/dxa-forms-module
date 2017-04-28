using Sdl.Web.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sdl.Web.Common.Models;
using System.Web.Helpers;
using Sdl.Web.Common.Logging;
using System.Text.RegularExpressions;
using Sdl.Web.Mvc.Configuration;
using DXA.Modules.Forms.Areas.Forms.Models;
using DXA.Modules.Forms.Areas.Forms.FormHandlers;
using DXA.Modules.Forms.Areas.Forms.Providers;

namespace DXA.Modules.Forms.Areas.Forms.Controllers
{
    public class FormsController : EntityController
    {
        protected override ViewModel EnrichModel(ViewModel model)
        {
            BaseFormModel baseForm = base.EnrichModel(model) as BaseFormModel;

            if (baseForm != null)
            {
                if (Request.HttpMethod == "POST")
                {
                    FormModel form = baseForm as FormModel;

                    // MapRequestFormData validates model as well
                    MapRequestFormData(form);

                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //Add logic to save form data here
                            foreach (var postAction in form.FormPostActions)
                            {
                                string formHandlerName = string.Empty;

                                try
                                {
                                    formHandlerName = postAction.FormHandler;

                                    var formHandler = FormHandlerRegistry.Get(formHandlerName);
                                    if (formHandler != null)
                                    {
                                        formHandler.ProcessForm(Request.Form, form.FormFields, postAction);
                                    }

                                    

                                }
                                catch (Exception ex)
                                {
                                    if (!string.IsNullOrEmpty(form.ErrorRedirect.Url))
                                    {
                                        Log.Error(ex, "Error occured while processing form data with form handler: {0}. Error message: {1}", formHandlerName, ex.Message);
                                        return new RedirectModel(form.ErrorRedirect.Url);
                                    }
                                    else
                                        throw ex;
                                }
                            }

                            if (!string.IsNullOrEmpty(form.SuccessRedirect.Url))
                                return new RedirectModel(form.SuccessRedirect.Url);
                            else
                                return new RedirectModel(WebRequestContext.Localization.Path);
                        }
                        catch (Exception ex)
                        {
                            if (!string.IsNullOrEmpty(form.ErrorRedirect.Url))
                            {
                                Log.Error(ex, "Error occured while saving form data.");
                                return new RedirectModel(form.ErrorRedirect.Url);
                            }
                            else
                                throw ex;
                        }

                    }
                }
                else
                {
                    // Here we can add logic to enrich our model

                    // Load options from category
                    foreach (var field in baseForm.FormFields.Where(f => !string.IsNullOrEmpty(f.OptionsCategory)))
                    {
                        if (field.FieldType == FieldType.DropDown || field.FieldType == FieldType.CheckBox || field.FieldType == FieldType.RadioButton)
                        {

                            List<OptionModel> options = TaxonomyProvider.LoadOptionsFromCategory(field.OptionsCategory, WebRequestContext.Localization);
                            field.OptionsCategoryList = options;
                        }
                    }
                    
                }
            }

            return baseForm;
        }

        [HttpPost]
        [Route("~/Forms/ProcessAjaxForm")]
        public ActionResult ProcessAjaxForm(FormCollection formCollection)
        {

            // get form entity by id
            // <param name="id">The Entity Identifier in format ComponentID-TemplateID.</param>
            string formId = formCollection["formId"];

            if (string.IsNullOrEmpty(formId))
            {
                throw new Exception("FormdId was not found in form values");
            }


            EntityModel entity = this.ContentProvider.GetEntityModel(formId, WebRequestContext.Localization);

            AjaxFormModel form = entity as AjaxFormModel;

            if (form != null)
            {

                MapRequestFormData(form);

                if (ModelState.IsValid)
                {
                    try {
                        //Add logic to save form data here
                        foreach (var postAction in form.FormPostActions)
                        {
                            string formHandlerName = string.Empty;
                            try
                            {
                                formHandlerName = postAction.FormHandler;

                                var formHandler = FormHandlerRegistry.Get(formHandlerName);
                                if (formHandler != null)
                                {
                                    formHandler.ProcessForm(formCollection, form.FormFields, postAction);
                                }
                            }
                            catch(Exception ex)
                            {
                                if (form.ErrorRedirect != null)
                                {
                                    Log.Error(ex, "Error occured while processing form data with form handler: {0}. Error message: {1}", formHandlerName, ex.Message);
                                    return PartialView("~/Areas/Forms/Views/Forms/ErrorResult.cshtml", form.ErrorRedirect);
                                }
                                else
                                {
                                    throw ex;
                                }
                            }
                        }

                        if (form.SuccessRedirect != null)
                            return PartialView("~/Areas/Forms/Views/Forms/SuccessResult.cshtml", form.SuccessRedirect);
                        else
                            return Redirect(WebRequestContext.Localization.Path);
                    }
                    catch (Exception ex)
                    {
                        if (form.ErrorRedirect != null)
                        {
                            Log.Error(ex, "Error occured while saving form data.");
                            return PartialView("~/Areas/Forms/Views/Forms/ErrorResult.cshtml", form.ErrorRedirect);
                        }
                        else
                            throw ex;
                    }

                }
                
            }

            return View();
        }

        protected new bool MapRequestFormData(FormModel model)
        {
            if (Request.HttpMethod != "POST")
            {
                return false;
            }

            // CSRF protection: If the anti CSRF cookie is present, a matching token must be in the form data too.
            const string antiCsrfToken = "__RequestVerificationToken";
            if (Request.Cookies[antiCsrfToken] != null)
            {
                AntiForgery.Validate();
            }


            foreach (string formField in Request.Form)
            {
                if (formField == antiCsrfToken)
                {
                    // This is not a form field, but the anti CSRF token (already validated above).
                    continue;
                }

                

                FormFieldModel fieldModel = model.FormFields.FirstOrDefault(f => f.Name == formField);
                if (fieldModel == null)
                {
                    Log.Debug("Form [{0}] has no defined field for form field '{1}'", model.Id, formField);
                    continue;
                }


                // TODO: validate if field is valid string (no injection etc)
                
                //string formFieldValue = Request.Form[formField];
                List<string> formFieldValues = Request.Form.GetValues(formField).Where(f => f != "false").ToList();
                fieldModel.Values = formFieldValues;
                 

                FormFieldValidator validator = new FormFieldValidator(fieldModel);
                string validationMessage = "Field Validation Failed";
                if (!validator.Validate(formFieldValues, ref validationMessage))
                {
                    if (validationMessage != null)
                    {
                        Log.Debug("Validation of property '{0}' failed: {1}", fieldModel.Name, validationMessage);
                        ModelState.AddModelError(fieldModel.Name, validationMessage);
                        continue;
                    }
                }

                try
                {
                    // set value to real model
                    //fieldModel.Value = formFieldValue;

                }
                catch (Exception ex)
                {
                    Log.Debug("Failed to set Model [{0}] property '{1}' to value obtained from form data: '{2}'. {3}", model.Id, fieldModel.Name, fieldModel.Value, ex.Message);
                    ModelState.AddModelError(fieldModel.Name, ex.Message);
                }

            }

            return true;
        }


    }

    public class FormFieldValidator
    {
        FormFieldModel field;

        public FormFieldValidator(FormFieldModel fieldDefinition)
        {
            field = fieldDefinition;
        }

        public bool Validate(List<string> values, ref string validationMessage)
        {

            string required = field.Required;

            string regexPattern = field.ValidationRegex;



            if (required == "Yes")
            {
                bool fieldValueProvided = false;
                if (values.Count > 0 && !string.IsNullOrEmpty(values.First()))
                    fieldValueProvided = true;

                if (!fieldValueProvided)
                {
                    if (!string.IsNullOrEmpty(field.RequiredError))
                    {
                        validationMessage = field.RequiredError;
                    }
                    else
                    {
                        validationMessage = "Field is required";
                    }
                    return false;
                }
            }

            if (regexPattern != null)
            {
                
                try
                {
                    Regex rgx = new Regex(regexPattern);
                    if (!rgx.IsMatch(values.First()))
                    {
                        if (!string.IsNullOrEmpty(field.ValidationError))
                        {
                            validationMessage = field.ValidationError;
                        }
                        else
                        {
                            validationMessage = "Field validation failed";
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    //TODO: add log message
                    return false;
                }
            }

            return true;
        }

    }


}