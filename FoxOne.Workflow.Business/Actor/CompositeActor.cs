/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/13 10:08:43
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoxOne.Workflow.Kernel;
using System.ComponentModel;
using FoxOne.Core;

namespace FoxOne.Workflow.Business
{
    [Serializable, DisplayName("复合参与者")]
    public class CompositeActor : BaseActor
    {
        /// <summary>
        /// 子参与者集合
        /// </summary>
        [DisplayName("子参与者集合"), DefaultValue(typeof(List<IActor>))]
        public IList<IActor> Actors { get; set; }

        /// <summary>
        /// 操作符号
        /// </summary>
        [DisplayName("操作符号")]
        public CompositeActorOperator Operator { get; set; }

        //
        // 摘要:
        /// <summary>
        /// 参与者的与或操作
        /// </summary>
        [DisplayName("计算方式")]
        public ActorOperatorType OperatorType { get; set; }


        public override IList<IUser> Resolve(IWorkflowContext context)
        {
            List<IUser> result = new List<IUser>();
            var comparer = new UserComparer();
            if (Actors == null || Actors.Count == 0)
            {
                throw new ArgumentNullException(string.Format("{0}:在步骤【{1}】求解参与者发生异常，复合参与者需要设置子参与者集合", context.FlowInstance.Id, Owner.Name));
            }
            bool bFirst = true;
            foreach (var a in Actors)
            {
                a.Owner = this.Owner;
                if (bFirst)
                {
                    try
                    {
                        var tempResult = a.Resolve(context);
                        if (tempResult != null && tempResult.Count > 0)
                        {
                            if (OperatorType == ActorOperatorType.Or)
                            {
                                return tempResult;
                            }
                            result.AddRange(tempResult);
                        }
                        bFirst = false;
                    }
                    catch { }
                    continue;
                }
                try
                {
                    IList<IUser> subresult = a.Resolve(context);
                    if (subresult != null)
                    {
                        if (OperatorType == ActorOperatorType.Or)
                        {
                            return subresult;
                        }

                        switch (Operator)
                        {
                            case CompositeActorOperator.Union:
                                result = new List<IUser>(result.Union(subresult, comparer));
                                break;
                            case CompositeActorOperator.Intersection:
                                result = new List<IUser>(result.Intersect(subresult, comparer));
                                break;
                            case CompositeActorOperator.Complement:
                                result = new List<IUser>(result.Except(subresult, comparer));
                                break;
                        }
                    }
                }
                catch { }
            }
            return result;
        }
    }

    public class UserComparer : IEqualityComparer<IUser>
    {
        public bool Equals(IUser x, IUser y)
        {
            return x.Id.Equals(y.Id, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(IUser obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public enum CompositeActorOperator
    {

        [Description("并")]
        Union = 0,

        [Description("交")]
        Intersection = 1,

        [Description("差")]
        Complement = 2
    }

    public enum ActorOperatorType
    {
        [Description("与")]
        And = 0,

        [Description("或")]
        Or = 1
    }
}
