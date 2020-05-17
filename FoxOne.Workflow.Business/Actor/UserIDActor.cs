/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 8:21:22
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using FoxOne.Workflow.Kernel;
using FoxOne.Core;
using FoxOne.Workflow.DataAccess;
using System.Threading;

namespace FoxOne.Workflow.Business
{
    [Serializable, DisplayName("用户ID参与者"),Browsable(false)]
    public class UserIDActor:BaseActor
    {
        public string UserIDs { get; set; }

        public string ParameterName { get; set; }

        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            string userIds = string.Empty;
            if (!string.IsNullOrEmpty(ParameterName))
            {
                if (context.Parameter.Keys.Contains(ParameterName, StringComparer.Create(Thread.CurrentThread.CurrentCulture, true)))
                {
                    userIds = context.Parameter[ParameterName].ToString();
                }
            }
            if (string.IsNullOrEmpty(userIds))
            {
                userIds = UserIDs;
            }
            if (!string.IsNullOrEmpty(userIds))
            {
                var userIdSplit = userIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                return DBContext<IUser>.Instance.Where(o => userIdSplit.Contains(o.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            }
            return null;
        }
    }
}
