using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FoxOne.Core
{
    public interface IPermission : IDURP
    {
        /// <summary>
        /// URL地址或控件ID
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// 所属父级
        /// </summary>
        string ParentId { get; set; }

        /// <summary>
        /// 行为：可见，不可见，可用，不可用。
        /// </summary>
        string Behaviour { get; set; }

        string Icon { get; set; }

        IPermission Parent { get; }

        PermissionType Type { get; set; }

        IEnumerable<IPermission> Childrens { get; }
    }

    public enum PermissionType
    {
        [Description("系统")]
        System = 10,

        [Description("模块")]
        Module = 0,

        [Description("页面")]
        Page = 1,

        [Description("控件")]
        Control = 2,

        [Description("规则")]
        Rule = 3
    }

}