using DXA.Modules.Forms.Areas.Forms.Models;
using Sdl.Web.Common.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DXA.Modules.Forms.Areas.Forms.FormHandlers
{
    public class EmailFormHandler : IFormHandler
    {
        public string Name
        {
            get
            {
                return "EmailHandler";
            }
        }

        public void ProcessForm(NameValueCollection formData, List<FormFieldModel> fields, BaseFormPostActionModel model)
        {
            try
            {
                EmailPostActionModel emailPostActionModel = model as EmailPostActionModel;

                SmtpClient client = new SmtpClient();
                //client.EnableSsl = true;

                MailMessage email = new MailMessage();

                foreach (var recipient in emailPostActionModel.To)
                {
                    email.To.Add(recipient);
                }

                foreach (var recipient in emailPostActionModel.Cc)
                {
                    email.CC.Add(recipient);
                }
                // TODO: define 
                email.From = new MailAddress("test@test.com");

                email.Subject = "DXA Module Form Data";

                email.Body = GenerateEmailBody(formData);

                client.Send(email);

            }
            catch (Exception ex)
            {
                Log.Debug(string.Format("Error while sending email: {0}", ex.Message));
                throw ex;
            }

        }

        public string GenerateEmailBody(NameValueCollection formData)
        {
            StringBuilder mailBuilder = new StringBuilder();

            foreach (string formField in formData)
            {
                mailBuilder.AppendLine(string.Format("{0}: {1}", formField, formData[formField]));
            }

            return mailBuilder.ToString();
        }
    }
}