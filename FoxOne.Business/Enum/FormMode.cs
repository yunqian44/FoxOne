using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FoxOne.Business
{
    public enum FormMode
    {
        [Description("新增")]
        Insert,

        [Description("编辑")]
        Edit,

        [Description("查看")]
        View
    }

    public enum ControlSecurityBehaviour
    {
        [Description("启用")]
        Enabled = 1,

        [Description("禁用")]
        Disabled = 2,

        [Description("可见")]
        Visible = 3,

        [Description("不可见")]
        Invisible = 4
    }
}
