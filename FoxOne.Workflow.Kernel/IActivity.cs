/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/1/26 14:14:26
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FoxOne.Business;

namespace FoxOne.Workflow.Kernel
{
    /// <summary>
    /// 流程步骤接口
    /// </summary>
    public interface IActivity : IControl
    {
        /// <summary>
        /// 步骤名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 步骤显示名称
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// 步骤所属的流程对象
        /// </summary>
        IWorkflow Owner { get; set; }

        /// <summary>
        /// 步骤参与者求解方式
        /// </summary>
        IActor Actor { get; set; }

        /// <summary>
        /// 步骤向外迁移的集合
        /// </summary>
        IList<ITransition> Transitions { get; set; }

        /// <summary>
        /// 指示在该步骤是否需要弹出选人界面
        /// </summary>
        bool NeedChoice { get; set; }

        /// <summary>
        /// 是否自动运行
        /// </summary>
        bool AutoRun { get;}

        /// <summary>
        /// 能否进入步骤
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool CanEnter(IWorkflowContext context);

        /// <summary>
        /// 能否运行步骤
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool CanExecute(IWorkflowContext context);

        /// <summary>
        /// 能否退出步骤
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool CanExit(IWorkflowContext context);

        /// <summary>
        /// Enter方法，通常用来生成一条或多条用户的待办
        /// </summary>
        /// <param name="context"></param>
        void Enter(IWorkflowContext context);

        /// <summary>
        /// Execute方法，通常用来将待办置为已办
        /// </summary>
        /// <param name="context"></param>
        void Execute(IWorkflowContext context);

        /// <summary>
        /// Exit方法，通常用来处理除当前工作项外同批次其它工作项的状态
        /// </summary>
        /// <param name="context"></param>
        void Exit(IWorkflowContext context);
    }
}
