using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sdl.Web.Common.Models;

namespace DXA.Modules.Forms.Areas.Forms.Models
{
    [SemanticEntity(EntityName ="Form", Prefix = "a", Vocab = "http://www.sdl.com/web/schemas/forms")]
    public class BaseFormModel : EntityModel
    {
        [SemanticProperty("a:formField")]
        public List<FormFieldModel> FormFields { get; set; }

        public string SubmitButtonLabel { get; set; }

        //[SemanticProperty("a:formPostAction")]
        public List<BaseFormPostActionModel> FormPostActions { get; set; }
    }
}