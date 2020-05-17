using FoxOne.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FoxOne.Data
{
    public class ExpressionTool
    {
        public Type TargetType { get; set; }

        public IDaoProvider Provider { get; set; }

        public IDictionary<string, object> Parameter { get; set; }

        /// <summary>
        /// 判断该MemberExpression表达式是否为访问目标类型的表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool IsTargetTypeMember(Expression expression)
        {
            var exp = expression as MemberExpression;
            if (exp == null)
            {
                return false;
            }
            var declaringType = exp.Member.DeclaringType;
            return declaringType == TargetType || TargetType.IsSubclassOf(declaringType);
        }

        /// <summary>
        /// 获取方法调用表达式的值
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private object GetMethodCallValue(Expression func)
        {
            object result1 = null;
            if (func.NodeType == ExpressionType.Call)
            {
                switch (func.Type.Name)
                {
                    case "Int32":
                        {
                            var getter = Expression.Lambda<Func<int>>(func).Compile();
                            result1 = getter();
                        }
                        break;
                    case "String":
                        {
                            var getter = Expression.Lambda<Func<string>>(func).Compile();
                            result1 = getter();
                        }
                        break;
                    case "DateTime":
                        {
                            var getter = Expression.Lambda<Func<DateTime>>(func).Compile();
                            result1 = getter();
                        }
                        break;
                    case "Boolean":
                        {
                            var getter = Expression.Lambda<Func<bool>>(func).Compile();
                            result1 = getter();
                        }
                        break;
                    default:
                        {
                            var getter = Expression.Lambda<Func<object>>(func).Compile();
                            result1 = getter();
                        }
                        break;
                }
            }
            return result1;
        }

        private object GetMemberCallValue(Expression func)
        {
            /*   当：m.LoginId.Contains("liuhf") 时，mce.Object:m.LoginId,mce.MethodName:Contains,mce.Arguments[0]:liuhf
                 当：m.LoginId.IsNullOrEmpty()时， mce.Object:null,mce.MethodName:IsNullOrEmpty,mce.Arguments[0]:m.LoginId
                 当：itemIds.Contains(o.ItemId)时，mce.Object:null,mce.MethodName:Contains,mce.Arguments:[0] itemIds [1] o.ItemId
             */
            var mce = func as MethodCallExpression;
            if (IsTargetTypeMember(mce.Object))
            {
                var mb = mce.Object as MemberExpression;
                if (mce.Arguments.Count > 0)
                {
                    Parameter[mb.Member.Name] = GetSqlByExpression(mce.Arguments[0]);
                    return GetMethodExpress(mce, mb);
                }
            }
            else
            {
                if (mce.Arguments != null && mce.Arguments.Count > 0)
                {
                    if (IsTargetTypeMember(mce.Arguments[0]))
                    {
                        return GetMethodExpress(mce, mce.Arguments[0] as MemberExpression);
                    }
                    if (mce.Arguments.Count > 1)
                    {
                        if (IsTargetTypeMember(mce.Arguments[1]))
                        {
                            var mb = mce.Arguments[1] as MemberExpression;
                            object value = GetSqlByExpression(mce.Arguments[0]);
                            if (value.GetType().IsArray)
                            {
                                var temp = value as int[];
                                if (temp != null)
                                {
                                    value = "'{0}'".FormatTo(String.Join("','", temp));
                                }
                                else
                                {
                                    var temp1 = value as string[];
                                    if (temp1 != null)
                                    {
                                        value = "'{0}'".FormatTo(String.Join("','", temp1));
                                    }
                                    else
                                    {
                                        throw new Exception("Not Suppost!");
                                    }
                                }
                            }
                            return GetMethodExpress(mce, mb, value);
                        }
                    }
                }
            }
            return GetMethodCallValue(func);
        }

        private string GetMethodExpress(MethodCallExpression mce, MemberExpression mb, object arrayValue = null)
        {
            string result = string.Empty;
            if (mce.Method.Name.Equals("Contains"))
            {
                if (arrayValue != null)
                {
                    result = "({0} IN ({1}))".FormatTo(Provider.EscapeIdentifier(mb.Member.Name), arrayValue);
                }
                else
                {
                    result = "({0} like '%${1}$%')".FormatTo(Provider.EscapeIdentifier(mb.Member.Name), mb.Member.Name);
                }
            }
            else if (mce.Method.Name.Equals("StartsWith"))
            {
                result = "({0} like '${1}$%')".FormatTo(Provider.EscapeIdentifier(mb.Member.Name), mb.Member.Name);
            }
            else if (mce.Method.Name.Equals("EndsWith"))
            {
                result = "({0} like '%${1}$')".FormatTo(Provider.EscapeIdentifier(mb.Member.Name), mb.Member.Name);
            }
            else if (mce.Method.Name.Equals("Equals"))
            {
                result = "({0} = #{1}#)".FormatTo(Provider.EscapeIdentifier(mb.Member.Name), mb.Member.Name);
            }
            else if (mce.Method.Name.Equals("IsNotNullOrEmpty"))
            {
                result = "({0} IS NOT NULL AND {0} <> '')".FormatTo(Provider.EscapeIdentifier(mb.Member.Name));
            }
            else if (mce.Method.Name.Equals("IsNullOrEmpty"))
            {
                result = "({0} IS NULL OR {0} = '')".FormatTo(Provider.EscapeIdentifier(mb.Member.Name));
            }
            else
            {
                throw new Exception("Not Suppost!");
            }
            return result;
        }

        private void GetLeftRight(Expression leftExp, Expression rightExp, ExpressionType type, bool notOp, out string left, out string op, out string right)
        {
            string paramName = string.Empty;
            left = GetSqlByExpression(leftExp).ToString();
            paramName = left;
            while (Parameter.Keys.Contains(paramName))
            {
                paramName += "1";
            }
            Parameter[paramName] = GetSqlByExpression(rightExp);
            right = Provider.NamedParameterFormat.FormatTo(paramName);
            op = GetOperator(type, notOp);
        }

        public object GetSqlByExpression(Expression func)
        {
            switch (func.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.NotEqual:
                    var exp = func as BinaryExpression;
                    string left, right, op;
                    bool leftIsBinary = exp.Left is BinaryExpression;
                    bool rightIsBinary = exp.Right is BinaryExpression;
                    var expLeft = exp.Left;
                    var expRight = exp.Right;
                    op = GetOperator(exp.NodeType);
                    if (!leftIsBinary && !rightIsBinary)
                    {
                        if (expLeft is MethodCallExpression && expRight is MethodCallExpression)
                        {
                            left = GetMemberCallValue(expLeft).ToString();
                            right = GetMemberCallValue(expRight).ToString();
                        }
                        else
                        {
                            if (expLeft.NodeType == ExpressionType.Convert)
                            {
                                expLeft = (expLeft as UnaryExpression).Operand;
                            }
                            if (expRight.NodeType == ExpressionType.Convert)
                            {
                                expRight = (expRight as UnaryExpression).Operand;
                            }
                            if (IsTargetTypeMember(expLeft))
                            {
                                GetLeftRight(expLeft, expRight, exp.NodeType, false, out left, out op, out right);
                            }
                            else if (IsTargetTypeMember(expRight))
                            {
                                GetLeftRight(expRight, expLeft, exp.NodeType, true, out left, out op, out right);
                            }
                            else
                            {
                                throw new Exception("不支持此表达式！");
                            }
                            left = Provider.EscapeIdentifier(left);
                        }
                    }
                    else
                    {
                        left = GetSqlByExpression(exp.Left).ToString();
                        right = GetSqlByExpression(exp.Right).ToString();
                    }
                    return "({0}{1}{2})".FormatTo(left, op, right);
                case ExpressionType.Constant:
                    var exp3 = func as ConstantExpression;
                    if (exp3.Value.ToString() == "True")
                    {
                        return " 1 = 1 ";
                    }
                    else if (exp3.Value.ToString() == "False")
                    {
                        return " 0 = 1 ";
                    }
                    else
                    {
                        return exp3.Value;
                    }
                case ExpressionType.MemberAccess:
                    var exp1 = func as MemberExpression;
                    if (exp1.Expression == null)
                    {
                        object value = null;
                        var isField = exp1.Member is System.Reflection.FieldInfo;
                        var isProperty = exp1.Member is System.Reflection.PropertyInfo;
                        if (isField)
                        {
                            value = GetFiledValue(exp1);
                        }
                        else if (isProperty)
                        {
                            value = GetPropertyValue(exp1);
                        }
                        return value;
                    }
                    else
                    {
                        if (exp1.Expression.NodeType == ExpressionType.Constant)
                        {
                            return GetMemberValue(exp1.Member, exp1);
                        }
                        else if (exp1.Expression.NodeType == ExpressionType.MemberAccess)
                        {
                            string memberName = exp1.Member.Name;
                            object tempResult = GetSqlByExpression(exp1.Expression);
                            if (tempResult.GetType().IsClass && tempResult.GetType() != typeof(string))
                            {
                                return FastType.Get(tempResult.GetType()).GetGetter(memberName).GetValue(tempResult);
                            }
                            else
                            {
                                return tempResult;
                            }
                        }
                        else
                        {
                            return exp1.Member.Name;
                        }
                    }
                case ExpressionType.Call:
                    return GetMemberCallValue(func);
                case ExpressionType.Not:
                case ExpressionType.Convert:
                    var result = GetOperator(func.NodeType);
                    return result + GetSqlByExpression((func as UnaryExpression).Operand);
                default:
                    throw new NotSupportedException("Not Suppost ExpressionType：" + func.NodeType.ToString());
            }
        }

        /// <summary>
        /// 根据表达式类型返回SQL语句的操作符，o.Age>3时不用取反，3>o.Age时，操作数与被操作数倒序时操作符要取反，即：o.Age<3
        /// </summary>
        /// <param name="expressiontype">操作类型</param>
        /// <param name="notOp">是否取反</param>
        /// <returns></returns>
        public string GetOperator(ExpressionType expressiontype, bool notOp = false)
        {
            switch (expressiontype)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return notOp ? "<" : ">";
                case ExpressionType.GreaterThanOrEqual:
                    return notOp ? "<=" : ">=";
                case ExpressionType.LessThan:
                    return notOp ? ">" : "<";
                case ExpressionType.LessThanOrEqual:
                    return notOp ? ">=" : "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    throw new FoxOneException("Not Suppost ExpressionType:".FormatTo(expressiontype.ToString()));
            }
        }

        public object GetMemberValue(MemberInfo member, Expression expression)
        {
            var rootExpression = expression as MemberExpression;
            var memberInfos = new Stack<MemberInfo>();
            var fieldInfo = member as System.Reflection.FieldInfo;
            object reval = null;
            MemberExpression memberExpr = null;
            while (expression is MemberExpression)
            {
                memberExpr = expression as MemberExpression;
                memberInfos.Push(memberExpr.Member);
                if (memberExpr.Expression == null)
                {
                    var isProperty = memberExpr.Member.MemberType == MemberTypes.Property;
                    var isField = memberExpr.Member.MemberType == MemberTypes.Field;
                    if (isProperty)
                    {
                        reval = GetPropertyValue(memberExpr);
                    }
                    else if (isField)
                    {
                        reval = GetFiledValue(memberExpr);
                    }
                }
                expression = memberExpr.Expression;
            }
            var constExpr = expression as ConstantExpression;
            object objReference = constExpr.Value;
            while (memberInfos.Count > 0)
            {
                var mi = memberInfos.Pop();
                if (mi.MemberType == MemberTypes.Property)
                {
                    var objProp = objReference.GetType().GetProperty(mi.Name);
                    if (objProp == null)
                    {
                        objReference = DynamicInvoke(expression, rootExpression == null ? memberExpr : rootExpression);
                    }
                    else
                    {
                        objReference = objProp.GetValue(objReference, null);
                    }
                }
                else if (mi.MemberType == MemberTypes.Field)
                {
                    var objField = objReference.GetType().GetField(mi.Name);
                    if (objField == null)
                    {
                        objReference = DynamicInvoke(expression, rootExpression == null ? memberExpr : rootExpression);
                    }
                    else
                    {
                        objReference = objField.GetValue(objReference);
                    }
                }
            }
            reval = objReference;
            return reval;
        }

        public object GetFiledValue(MemberExpression memberExpr)
        {
            object reval = null;
            FieldInfo field = (FieldInfo)memberExpr.Member;
            reval = field.GetValue(memberExpr.Member);
            if (reval != null && reval.GetType().IsClass && reval.GetType() != typeof(string))
            {
                var fieldName = memberExpr.Member.Name;
                var proInfo = reval.GetType().GetProperty(fieldName);
                if (proInfo != null)
                {
                    reval = proInfo.GetValue(reval, null);
                }
                var fieInfo = reval.GetType().GetField(fieldName);
                if (fieInfo != null)
                {
                    reval = fieInfo.GetValue(reval);
                }
            }
            return reval;
        }


        public bool IsConstExpression(MemberExpression memberExpr)
        {
            var result = false;
            while (memberExpr != null && memberExpr.Expression != null)
            {
                var isConst = memberExpr.Expression is ConstantExpression;
                if (isConst)
                {
                    result = true;
                    break;
                }
                memberExpr = memberExpr.Expression as MemberExpression;
            }
            return result;
        }

        public object GetPropertyValue(MemberExpression memberExpr)
        {
            object reval = null;
            PropertyInfo pro = (PropertyInfo)memberExpr.Member;
            reval = pro.GetValue(memberExpr.Member, null);
            if (reval != null && reval.GetType().IsClass && reval.GetType() != typeof(string))
            {
                var fieldName = memberExpr.Member.Name;
                var proInfo = reval.GetType().GetProperty(fieldName);
                if (proInfo != null)
                {
                    reval = proInfo.GetValue(reval, null);
                }
                var fieInfo = reval.GetType().GetField(fieldName);
                if (fieInfo != null)
                {
                    reval = fieInfo.GetValue(reval);
                }
            }
            return reval;
        }

        public object DynamicInvoke(Expression expression, MemberExpression memberExpression = null)
        {
            object value = Expression.Lambda(expression).Compile().DynamicInvoke();
            if (value != null && value.GetType().IsClass && value.GetType() != typeof(string) && memberExpression != null)
            {
                value = Expression.Lambda(memberExpression).Compile().DynamicInvoke();
            }

            return value;
        }

        public Type GetPropertyOrFieldType(MemberInfo propertyOrField)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                return ((PropertyInfo)propertyOrField).PropertyType;
            if (propertyOrField.MemberType == MemberTypes.Field)
                return ((FieldInfo)propertyOrField).FieldType;
            throw new NotSupportedException();
        }
    }
}
