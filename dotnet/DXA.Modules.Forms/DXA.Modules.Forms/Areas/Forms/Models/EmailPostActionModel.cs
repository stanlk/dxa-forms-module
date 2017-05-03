using Sdl.Web.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DXA.Modules.Forms.Areas.Forms.Models
{
    [SemanticEntity(EntityName = "EmailPostAction", Prefix="e", Vocab = CoreVocabulary)]
    public class EmailPostActionModel : BaseFormPostActionModel
    {
        [SemanticProperty("e:to")]
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
    }
}