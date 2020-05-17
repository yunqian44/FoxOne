/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/25 16:30:31
 * 描述说明：
 * *******************************************************/
using System;
namespace FoxOne.Workflow.Kernel
{
    public interface IWorkDayService
    {
        TimeSpan GetSpan(DateTime start, DateTime end);
        IWorkDay[] GetSpecialDays(DateTime start, DateTime end);
        DateTime GetTimeAfterSpan(DateTime start, TimeSpan span);
        void SetDays(IWorkDay[] days);
    }
}
