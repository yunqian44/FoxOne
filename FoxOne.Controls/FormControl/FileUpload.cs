using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FoxOne.Controls
{
    [DisplayName("文件上传")]
    public class FileUpload : FormControlBase
    {

        public FileUpload()
        {
            EditColSpan = true;
        }

        protected override string TagName
        {
            get
            {
                return "input";
            }
        }

        internal override void AddAttributes()
        {
            base.AddAttributes();
            Attributes["type"] = "file";
        }
    }

}
