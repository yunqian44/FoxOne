using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Controls
{
    public static class TemplateGenerator
    {
        public static string GetDefaultFormTemplate()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("<form action=\"{0}\" defaultForm=\"true\" method=\"post\" class=\"{1}\" enctype=\"multipart/form-data\">");
            result.AppendLine("{2}{3}");
            result.AppendLine("<div class=\"form-group\"><label>&nbsp;</label>{4}</div>");
            result.AppendLine("</form>");
            return result.ToString();
        }

        public static string GetSearchFormTemplate()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("<form searchForm=\"true\" method=\"post\" class=\"{0}\">");
            result.AppendLine("<div class=\"input-control\">");
            result.AppendLine("{1}");
            result.AppendLine("</div>");
            result.AppendLine("{2}");
            result.AppendLine("</form>");
            return result.ToString();
        }

        public static string GetFormFieldTemplate()
        {
            return "<div class=\"form-group\" description=\"{3}\"><label for=\"{0}\">{1}：</label>\n{2}\n</div>";
        }

        public static string GetFormGroupTemplate()
        {
            return "<div class=\"form-inline\">\n{0}\n</div>";
        }

        public static string GetPagerTemplate()
        {
            return "<div class=\"data-pager-left\"><ul>{0}</ul></div><div class=\"data-pager-right\"><span class=\"data-pager-right-number\">每页</span><ul> {1} </ul><span class=\"data-pager-right-number\">行</span></div>";
        }

        public static string GetPagerItemTemplate()
        {
            return "<li><a pageIndex=\"{0}\" {1}>{2}</a></li>";
        }

        public static string GetPagerSizeTemplate() 
        {
            return "<li><a pageSize=\"{0}\" {1}>{2}</a></li>";
        }

        public static string GetFormTableTemplate(IList<FormControlBase> f)
        {
            var fields = f.Where(o => o.Visiable).OrderBy(o => o.Rank).ToList();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendFormat("<table class=\"formitem\"><caption>{0}</caption>", "基础信息");
            builder.AppendLine();
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.EditColSpan)
                {
                    builder.AppendFormat("\t<tr><td>{0}：</td><td colspan=\"3\">#{1}#</td></tr>", field.Label, field.Id);
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendFormat("\t<tr><td>{0}：</td><td>#{1}#</td><td>", field.Label, field.Id);
                    if ((i + 1) < fields.Count && !fields[i + 1].EditColSpan)
                    {
                        i = i + 1;
                        field = fields[i];
                        builder.AppendFormat("{0}：</td><td>#{1}#</td></tr>", field.Label, field.Id);
                    }
                    else
                    {
                        builder.Append("&nbsp;</td><td>&nbsp;</td></tr>");
                    }
                    builder.AppendLine();
                }
            }
            builder.Append("</table>");
            builder.AppendLine();
            return builder.ToString();
        }
    }
}
