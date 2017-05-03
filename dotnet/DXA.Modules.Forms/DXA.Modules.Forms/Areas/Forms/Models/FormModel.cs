using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sdl.Web.Common.Models;

namespace DXA.Modules.Forms.Areas.Forms.Models
{
    [SemanticEntity(EntityName ="Form", Prefix = "a", Vocab = "http://www.sdl.com/web/schemas/forms")]
    public class FormModel : BaseFormModel
    {
        public string Heading { get; set; }

        public RichText Subheading { get; set; }
        
        public Link SuccessRedirect { get; set; }
        public Link ErrorRedirect { get; set; }
    }
}