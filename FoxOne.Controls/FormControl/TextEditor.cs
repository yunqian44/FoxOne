using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using FoxOne.Core;
namespace FoxOne.Controls
{

    /// <summary>
    /// 文本编辑器
    /// </summary>
    [DisplayName("文本编辑器")]
    public class TextEditor : TextArea
    {
        public TextEditor()
        {
            CssClass = "form-control xh-editor";
            EditColSpan = true;
        }


        internal override void AddAttributes()
        {
            base.AddAttributes();
            Attributes["TextEditor"] = "TextEditor";
        }
    }
}
