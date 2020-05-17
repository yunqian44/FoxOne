using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FoxOne.Core;
using FoxOne.Data;
using FoxOne.Business;
using FoxOne.Business.Environment;
using FoxOne.Workflow.Kernel;
using FoxOne.Workflow.Business;
using FoxOne.Workflow.DataAccess;
using System.Threading.Tasks;
namespace FoxOne.Web
{
    public static class RegisterCenter
    {
        private static bool HasRegisterType = false;
        private static bool HasRegisterEntityEvent = false;
        private readonly static object obj = new object();
        public static void RegisterType()
        {
            if (!HasRegisterType)
            {
                lock (obj)
                {
                    if (!HasRegisterType)
                    {
                        ObjectHelper.RegisterType<ILangProvider, RLangProvider>();
                        ObjectHelper.RegisterType<ICache, MemCache>();
                        ObjectHelper.RegisterType<IEmailSender, SmtpEmailSender>();

                        ObjectHelper.RegisterType<IDepartment, Department>();
                        ObjectHelper.RegisterType<IUser, User>();
                        ObjectHelper.RegisterType<IRole, Role>();
                        ObjectHelper.RegisterType<IPermission, Permission>();
                        ObjectHelper.RegisterType<IRoleType, RoleType>();
                        ObjectHelper.RegisterType<IUserRole, UserRole>();
                        ObjectHelper.RegisterType<IRolePermission, RolePermission>();
                        ObjectHelper.RegisterType<IRoleTypePermission, RoleTypePermission>();
                        ObjectHelper.RegisterType<IDURPProperty, DURPProperty>();

                        ObjectHelper.RegisterType<ISqlParameters, EnvParameters>("Env");
                        ObjectHelper.RegisterType<ISqlParameters, HttpContextProvider>("HttpContext");

                        ObjectHelper.RegisterType<IWorkflow, BusinessWorkflow>();
                        ObjectHelper.RegisterType<IWorkflowBuilder, WorkflowBuilder>();
                        ObjectHelper.RegisterType<IWorkDay, WorkDay>();
                        ObjectHelper.RegisterType<ITransition, BusinessTransition>();
                        ObjectHelper.RegisterType<IWorkflowInstance, WorkflowInstance>();
                        ObjectHelper.RegisterType<IWorkflowItem, WorkflowItem>();
                        ObjectHelper.RegisterType<IWorkflowItem, WorkflowItemRead>("Read");
                        ObjectHelper.RegisterType<IWorkflowParameter, WorkflowParameter>();
                        ObjectHelper.RegisterType<IWorkflowApplication, WorkflowApplication>();
                        ObjectHelper.RegisterType<IWorkflowDefinition, WorkflowDefinition>();
                        ObjectHelper.RegisterType<IWorkflowContext, WorkflowContext>();
                        ObjectHelper.RegisterType<IWorkflowChoice, WorkflowChoice>();
                        ObjectHelper.RegisterType<IWorkflowInstanceService, WorkflowInstanceService>();
                        ObjectHelper.RegisterType<IWorkDayService, WorkDayService>();

                        ObjectHelper.RegisterType(typeof(IService<>), typeof(CommonService<>));

                        HasRegisterType = true;
                    }
                }
            }
        }

        public static void RegisterEntityEvent()
        {
            if (!HasRegisterEntityEvent)
            {
                lock (obj)
                {
                    if (!HasRegisterEntityEvent)
                    {
                        EntityEventManager.RegisterEvent<IDepartment>(EventStep.Before, EventType.Insert, d =>
                        {
                            var o = d as IDepartment;
                            if (DBContext<IDepartment>.Instance.Where(k => k.Name.Equals(o.Name, StringComparison.OrdinalIgnoreCase)).Count() > 0)
                            {
                                throw new FoxOneException("Department_Name_Already_In_Use");
                            }
                            o.Alias = o.Name;
                            o.Level = o.Parent.Level + 1;
                            o.RentId = 1;
                            o.WBS = o.Parent.WBS + (o.Parent.Childrens.Count() + 1).ToString().PadLeft(3, '0');
                            return true;
                        });



                        EntityEventManager.RegisterEvent<IDepartment>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as IDepartment;
                            o.Member.ForEach(k =>
                            {
                                DBContext<IUser>.Delete(k);
                            });
                            o.Roles.ForEach(k =>
                            {
                                DBContext<IRole>.Delete(k);
                            });
                            o.Childrens.ForEach(k =>
                            {
                                DBContext<IDepartment>.Delete(k);
                            });
                            return true;
                        });

                        EntityEventManager.RegisterEvent<IUser>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as IUser;
                            var userRoles = DBContext<IUserRole>.Instance.Where(j => j.UserId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            userRoles.ForEach(k =>
                            {
                                DBContext<IUserRole>.Delete(k);
                            });
                            return true;
                        });


                        EntityEventManager.RegisterEvent<IUser>(EventStep.Before, EventType.Insert, d =>
                        {
                            var o = d as IUser;
                            if (DBContext<IUser>.Instance.Where(k => k.LoginId.Equals(o.LoginId, StringComparison.OrdinalIgnoreCase) || k.MobilePhone == o.MobilePhone).Count() > 0)
                            {
                                throw new FoxOneException("LoginId_Or_MobilePhone_Alerady_Exist");
                            }
                            return true;
                        });

                        EntityEventManager.RegisterEvent<IRole>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as IRole;
                            var userRoles = DBContext<IUserRole>.Instance.Where(j => j.RoleId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            userRoles.ForEach(k =>
                            {
                                DBContext<IUserRole>.Delete(k);
                            });
                            var permissions = DBContext<IRolePermission>.Instance.Where(j => j.RoleId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            permissions.ForEach(k =>
                            {
                                DBContext<IRolePermission>.Delete(k);
                            });
                            return true;
                        });


                        EntityEventManager.RegisterEvent<IRole>(EventStep.Before, EventType.Insert, d =>
                        {
                            var o = d as IRole;
                            var roles = DBContext<IRole>.Instance.Where(j => j.RoleTypeId.Equals(o.RoleTypeId, StringComparison.OrdinalIgnoreCase)
                                && j.DepartmentId.Equals(o.DepartmentId, StringComparison.OrdinalIgnoreCase));
                            if (roles.Count() > 0)
                            {
                                throw new FoxOneException("Role_Alerady_Exist");
                            }
                            return true;
                        });

                        EntityEventManager.RegisterEvent<IRoleType>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as IRoleType;
                            o.Roles.ForEach(k =>
                            {
                                DBContext<IRole>.Delete(k);
                            });
                            var permissions = DBContext<IRoleTypePermission>.Instance.Where(j => j.RoleTypeId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            permissions.ForEach(k =>
                            {
                                DBContext<IRoleTypePermission>.Delete(k);
                            });
                            return true;
                        });

                        EntityEventManager.RegisterEvent<IPermission>(EventStep.Before, EventType.Insert, d =>
                        {
                            var o = d as IPermission;
                            var temp = DBContext<IPermission>.Instance.FirstOrDefault(j => j.Code.Equals(o.Code, StringComparison.OrdinalIgnoreCase));
                            if (temp != null)
                            {
                                throw new FoxOneException("Permission_Code_Exist");
                            }
                            return true;
                        });

                        EntityEventManager.RegisterEvent<IPermission>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as IPermission;
                            var roleTypePermission = DBContext<IRoleTypePermission>.Instance.Where(j => j.PermissionId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            roleTypePermission.ForEach(k =>
                            {
                                DBContext<IRoleTypePermission>.Delete(k);
                            });

                            var rolePermission = DBContext<IRolePermission>.Instance.Where(j => j.PermissionId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            rolePermission.ForEach(k =>
                            {
                                DBContext<IRolePermission>.Delete(k);
                            });
                            return true;
                        });

                        EntityEventManager.RegisterEvent<PageEntity>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as PageEntity;
                            var components = DBContext<ComponentEntity>.Instance.Where(i => i.PageId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            if (!components.IsNullOrEmpty())
                            {
                                components.ForEach(k =>
                                {
                                    DBContext<ComponentEntity>.Delete(k);
                                });
                            }
                            var pageFile = DBContext<PageLayoutFileEntity>.Instance.Where(i => i.PageOrLayoutId.Equals(o.Id, StringComparison.OrdinalIgnoreCase)).ToList();
                            if (!pageFile.IsNullOrEmpty())
                            {
                                pageFile.ForEach(k =>
                                {
                                    DBContext<PageLayoutFileEntity>.Delete(k);
                                });
                            }
                            return true;
                        });

                        EntityEventManager.RegisterEvent<IWorkflowApplication>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as IWorkflowApplication;
                            if (DBContext<IWorkflowInstance>.Instance.Count(c => c.ApplicationId == o.Id) > 0)
                            {
                                throw new FoxOneException("当前流程应用有关联的实例，不能删除！");
                            }
                            return true;
                        });

                        EntityEventManager.RegisterEvent<IWorkflowDefinition>(EventStep.Before, EventType.Delete, d =>
                        {
                            var o = d as IWorkflowDefinition;
                            if (DBContext<IWorkflowApplication>.Instance.Count(c => c.WorkflowId == o.Id) > 0)
                            {
                                throw new FoxOneException("当前流程定义有关联的流程应用，不能删除！");
                            }
                            var components = DBContext<ComponentEntity>.Instance.Where(i => i.PageId.Equals(o.Id, StringComparison.OrdinalIgnoreCase));
                            if (!components.IsNullOrEmpty())
                            {
                                components.ForEach(k =>
                                {
                                    DBContext<ComponentEntity>.Delete(k);
                                });
                            }
                            return true;
                        });
                        HasRegisterEntityEvent = true;
                    }
                }
            }

        }
    }
}