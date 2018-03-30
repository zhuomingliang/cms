﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using SiteServer.Utils;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Utility;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "执行动作", Description = "通过 stl:action 标签在模板中创建链接，点击链接后将执行相应的动作")]
    public class StlAction
    {
        private StlAction() { }
        public const string ElementName = "stl:action";

        public const string AttributeType = "type";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {AttributeType, StringUtils.SortedListToAttributeValueString("动作类型", TypeList)}
        };

        public const string TypeTranslate = "Translate";
        public const string TypeClose = "Close";

        public static SortedList<string, string> TypeList => new SortedList<string, string>
        {
            {TypeTranslate, "繁体/简体转换"},
            {TypeClose, "关闭页面"}
        };

        public static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
        {
            var type = string.Empty;

            foreach (var name in contextInfo.Attributes.Keys)
            {
                var value = contextInfo.Attributes[name];
                if (StringUtils.EqualsIgnoreCase(name, AttributeType))
                {
                    type = value;
                }
            }

            return ParseImpl(pageInfo, contextInfo, type);
        }

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, string type)
        {
            var stlAnchor = new HtmlAnchor();

            foreach (var attributeName in contextInfo.Attributes.Keys)
            {
                stlAnchor.Attributes.Add(attributeName, contextInfo.Attributes[attributeName]);
            }

            var url = PageUtils.UnclickedUrl;
            var onclick = string.Empty;

            var innerBuilder = new StringBuilder(contextInfo.InnerXml);
            StlParserManager.ParseInnerContent(innerBuilder, pageInfo, contextInfo);
            stlAnchor.InnerHtml = innerBuilder.ToString();

            //计算动作开始
            if (!string.IsNullOrEmpty(type))
            {
                if (StringUtils.EqualsIgnoreCase(type, TypeTranslate))
                {
                    pageInfo.AddPageBodyCodeIfNotExists(PageInfo.Const.JsAhTranslate);

                    var msgToTraditionalChinese = "繁體";
                    var msgToSimplifiedChinese = "简体";
                    if (!string.IsNullOrEmpty(stlAnchor.InnerHtml))
                    {
                        if (stlAnchor.InnerHtml.IndexOf(",", StringComparison.Ordinal) != -1)
                        {
                            msgToTraditionalChinese = stlAnchor.InnerHtml.Substring(0, stlAnchor.InnerHtml.IndexOf(",", StringComparison.Ordinal));
                            msgToSimplifiedChinese = stlAnchor.InnerHtml.Substring(stlAnchor.InnerHtml.IndexOf(",", StringComparison.Ordinal) + 1);
                        }
                        else
                        {
                            msgToTraditionalChinese = stlAnchor.InnerHtml;
                        }
                    }
                    stlAnchor.InnerHtml = msgToTraditionalChinese;

                    if (string.IsNullOrEmpty(stlAnchor.ID))
                    {
                        stlAnchor.ID = "translateLink";
                    }

                    pageInfo.FootCodes[TypeTranslate] = $@"
<script type=""text/javascript""> 
var defaultEncoding = 0;
var translateDelay = 0;
var cookieDomain = ""/"";
var msgToTraditionalChinese = ""{msgToTraditionalChinese}"";
var msgToSimplifiedChinese = ""{msgToSimplifiedChinese}"";
var translateButtonId = ""{stlAnchor.ClientID}"";
translateInitilization();
</script>
";
                }
                else if (StringUtils.EqualsIgnoreCase(type, TypeClose))
                {
                    url = "javascript:window.close()";
                }
            }
            //计算动作结束

            stlAnchor.HRef = url;

            if (!string.IsNullOrEmpty(onclick))
            {
                stlAnchor.Attributes.Add("onclick", onclick);
            }

            // 如果是实体标签，则只返回url
            return contextInfo.IsStlEntity ? stlAnchor.HRef : ControlUtils.GetControlRenderHtml(stlAnchor);
        }
    }
}
