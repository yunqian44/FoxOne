/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/11 0:21:59
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Data;
using FoxOne.Workflow.Kernel;
using FoxOne.Data.Attributes;
using FoxOne.Core;
using System.ComponentModel;
using FoxOne.Business;

namespace FoxOne.Workflow.DataAccess
{
    public class WorkDayService : IWorkDayService
    {
        public virtual TimeSpan GetSpan(DateTime start, DateTime end)
        {
            TimeSpan result = (end - start).Add(new TimeSpan(1, 0, 0, 0));
            IWorkDay[] spcDays = GetSpecialDays(start, end);

            int days = (int)result.TotalDays;
            IWorkDay day = null;
            for (int i = 0; i < days; i++)
            {
                DateTime tmp = start.AddDays(i);
                if (spcDays != null)
                {
                    day = (from t in spcDays
                           where t.Day.Date == tmp.Date
                           select t).FirstOrDefault();
                }
                //非工作日
                if (((tmp.DayOfWeek == DayOfWeek.Saturday || tmp.DayOfWeek == DayOfWeek.Sunday) &&
                    ((day == null) || !day.IsWork)) ||
                    (((tmp.DayOfWeek != DayOfWeek.Saturday && tmp.DayOfWeek != DayOfWeek.Sunday)
                    && ((day != null) && !day.IsWork))))
                    result = result.Subtract(new TimeSpan(1, 0, 0, 0));
            }

            return result;
        }

        public virtual IWorkDay[] GetSpecialDays(DateTime start, DateTime end)
        {
            IDictionary<string, object> param = new Dictionary<string, object> { { "start", start.ToString("yyyy-MM-dd 00:00:00") }, { "end", end.ToString("yyyy-MM-dd 00:00:00") } };
            WorkDay[] workday = DBContext<WorkDay>.Instance.Where(o => o.Day >= start && o.Day <= end).ToArray();
            return workday;
        }

        public virtual DateTime GetTimeAfterSpan(DateTime start, TimeSpan span)
        {
            int days = (int)span.TotalDays;
            int i = 0;
            DateTime result = start;
            while (days >= 0)
            {
                result = start.AddDays(i);
                IWorkDay[] tmpDays = GetSpecialDays(result, result);
                //工作日就跳过
                if (((result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday) &&
                    ((tmpDays != null && tmpDays.Length != 0) && tmpDays[0].IsWork)) ||
                    (((result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday)
                    && ((tmpDays == null || tmpDays.Length == 0) || tmpDays[0].IsWork))))
                {
                    days--;
                }
                i++;
            }
            return result;
        }

        public virtual void SetDays(IWorkDay[] days)
        {
            string defaultDeptId = null;
            foreach (WorkDay day in days)
            {
                if (Guid.Empty.Equals(day.DepartmentId))
                {
                    if (string.IsNullOrEmpty(defaultDeptId))
                    {
                        defaultDeptId = DBContext<IDepartment>.Instance.FirstOrDefault(o => o.ParentId.IsNullOrEmpty()).Id;
                    }
                    day.DepartmentId = defaultDeptId;
                }
                WorkDay workDay = DBContext<WorkDay>.Instance.FirstOrDefault(o => o.Day == day.Day && o.DepartmentId.Equals(day.DepartmentId, StringComparison.OrdinalIgnoreCase));
                if (workDay == null)
                {
                    DBContext<WorkDay>.Insert(day);
                }
                else if ((day.Day.DayOfWeek != DayOfWeek.Saturday && day.Day.DayOfWeek != DayOfWeek.Sunday && day.IsWork) ||
                    ((day.Day.DayOfWeek == DayOfWeek.Sunday || day.Day.DayOfWeek == DayOfWeek.Saturday) && !day.IsWork))
                {
                    DBContext<WorkDay>.Delete(day);
                }
                else if ((day.Day.DayOfWeek != DayOfWeek.Saturday && day.Day.DayOfWeek != DayOfWeek.Sunday && !day.IsWork) ||
                    ((day.Day.DayOfWeek == DayOfWeek.Sunday || day.Day.DayOfWeek == DayOfWeek.Saturday) && day.IsWork))
                {
                    DBContext<WorkDay>.Update(day);   
                }
            }
        }
    }


    [DisplayName("工作日")]
    [Table("WFL_WorkDay")]
    public class WorkDay:EntityBase, IWorkDay,IAutoCreateTable
    {
        public DateTime Day { set; get; }

        public bool IsWork { set; get; }

        public string DepartmentId { set; get; }

        public string Description { set; get; }

    }
}
