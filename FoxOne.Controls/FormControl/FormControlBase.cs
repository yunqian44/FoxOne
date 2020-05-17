using FoxOne.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using FoxOne.Core;
namespace FoxOne.Controls
{
    public abstract class FormControlBase : ComponentBase, IFormControl
    {

        private TagBuilder _tagBuilder;
        public FormControlBase()
        {
            Enable = true;
            Visiable = true;
            CanModity = true;
            _tagBuilder = new TagBuilder(TagName);
            CssClass = "form-control";
        }

        protected abstract string TagName
        {
            get;
        }

        public string Name
        {
            get;
            set;
        }

        public string Validator
        {
            get;
            set;
        }

        [DisplayName("是否启用")]
        public bool Enable
        {
            get;
            set;
        }

        public virtual string Value
        {
            get;
            set;
        }

        [DisplayName("是否跨行")]
        public bool EditColSpan { get; set; }

        [FormField(Editable =false)]
        public bool CanModity { get; set; }

        [DisplayName("标签")]
        public string Label { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        [DisplayName("填写说明")]
        public string Description { get; set; }

        [DisplayName("自动触发搜索")]
        public bool ChangeTiggerSearch { get; set; }

        /// <summary>
        /// HTML容器
        /// </summary>
        [DisplayName("HTML容器")]
        public string ContainerTemplate { get; set; }

        /// <summary>
        /// 扩展属性
        /// </summary>
        [DisplayName("扩展属性")]
        public string AttributeString
        {
            get;
            set;
        }

        public IDictionary<string, string> Attributes
        {
            get
            {
                return _tagBuilder.Attributes;
            }
        }

        public override string Render()
        {
            if (Visiable)
            {
                AddAttributes();
                _tagBuilder.InnerHtml = RenderInner();
                string result = string.Empty;
                if (SelfClosing)
                {
                    result = _tagBuilder.ToString(TagRenderMode.SelfClosing);
                }
                else
                {
                    result = _tagBuilder.ToString();
                }
                result += RenderAfter();
                if (ContainerTemplate.IsNullOrEmpty())
                {
                    return result;
                }
                else
                {
                    return ContainerTemplate.FormatTo(Id, Label, result, Description);
                }
            }
            return string.Empty;
        }

        protected virtual bool SelfClosing
        {
            get
            {
                return false;
            }
        }

        protected virtual string RenderAfter()
        {
            return string.Empty;
        }

        protected virtual string RenderInner()
        {
            return string.Empty;
        }

        internal virtual void AddAttributes()
        {
            Attributes["id"] = Id;
            Attributes["name"] = Name;
            if (!Enable)
            {
                Attributes["disabled"] = "disabled";
            }
            if (!CssClass.IsNullOrEmpty())
            {
                Attributes["class"] = CssClass;
            }
            if (!Validator.IsNullOrEmpty())
            {
                Attributes["validator"] = Validator;
                Attributes["validatorFieldLabel"] = Label;
            }
            if (!AttributeString.IsNullOrEmpty())
            {
                string[] kvs = AttributeString.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var kv in kvs)
                {
                    string[] keyValue = kv.Split(':');
                    Attributes[keyValue[0]] = keyValue[1];
                }
            }
            if (ChangeTiggerSearch)
            {
                Attributes["data-autotigger"] = "true";
            }
        }
    }
}
