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

namespace FoxOne.Workflow.Business
{
   [Serializable, DisplayName("父级部门参与者")]
    public class ParentDepartmentActor:RoleActor
    {
       [DisplayName("部门级别")]
       public int Level
       {
           get;
           set;
       }

       [DisplayName("循环次数")]
       public int LoopCount
       {
           get;
           set;
       }

       protected override IDepartment GetBaseDepartment(IWorkflowContext context)
       {
           var department = base.GetBaseDepartment(context);
           if (Level == 0 && LoopCount>0)
           {
               while (LoopCount > 0)
               {
                   department = department.Parent;
                   LoopCount--;
               }
           }
           else
           {
               if (Level > 0)
               {
                   if (department.Level < Level)
                   {
                       throw new Exception("指定LEVEL所在部门为基准部门的下级");
                   }
                   while (department.Level != Level)
                   {
                       department = department.Parent;
                   }
               }
           }
           return department;
       }
    }
}
