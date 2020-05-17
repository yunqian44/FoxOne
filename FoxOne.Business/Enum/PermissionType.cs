using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FoxOne.Business
{
    public enum PermissionType
    {
        [Description("模块")]
        Module,

        [Description("页面")]
        Page,

        [Description("控件")]
        Control,

        [Description("规则")]
        Rule
    }
}
