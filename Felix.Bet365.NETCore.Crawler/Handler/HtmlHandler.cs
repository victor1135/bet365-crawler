using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Handler
{
    public class HtmlHandler
    {
        protected string _logPrefix;

        protected ILogger Logger;

        private HtmlNode _currentNode;
        public HtmlNode CurrentNode
        {
            get
            {
                return this._currentNode;
            }
            set
            {
                this._currentNode = value;
            }
        }

        public string InnerHtml
        {
            get
            {
                if (this._currentNode != null)
                {
                    return this._currentNode.InnerHtml;
                }
                return null;
            }
        }

        public string Attribute(string attribute)
        {
            if (this._currentNode != null && this._currentNode.Attributes.Contains(attribute))
            {
                return this._currentNode.Attributes[attribute].Value;
            }
            return null;
        }

        public HtmlHandler(string logPrefix, string content)
        {
            this._logPrefix = logPrefix;
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);
            this._currentNode = htmlDocument.DocumentNode;
        }

        public HtmlHandler(string logPrefix, HtmlNode node)
        {
            this._logPrefix = logPrefix;
            this._currentNode = node;
        }

        public static HtmlHandler GetImplement(string logPrefix, string content)
        {
            return new HtmlHandler(logPrefix, content);
        }

        public static HtmlHandler GetImplement(string logPrefix, HtmlNode node)
        {
            return new HtmlHandler(logPrefix, node);
        }

        public bool Any()
        {
            return this.Any(null);
        }
        public bool Any(Func<XPathHandler, XPathHandler> xpath)
        {
            XPathHandler xpathHandler = new XPathHandler();

            var node = this._currentNode;
            if (xpath != null)
            {
                node = node.SelectSingleNode(xpath(xpathHandler).GetPath());
            }
            if (node != null)
            {
                return HtmlHandler.GetImplement(_logPrefix, node) != null;
            }
            return false;
        }

        public string GetFromAttribute(Func<XPathHandler, XPathHandler> xpath, string attribute)
        {
            return this.GetFromAttribute(xpath, attribute, null);
        }

        public string GetFromAttribute(Func<XPathHandler, XPathHandler> xpath, string attribute, Func<string, string> strHandler)
        {
            XPathHandler xpathHandler = new XPathHandler();

            var node = this._currentNode;
            if (xpath != null)
            {
                node = node.SelectSingleNode(xpath(xpathHandler).GetPath());
            }

            if (node != null && node.Attributes.Contains(attribute))
            {
                return this.StrHandle(strHandler, node.Attributes[attribute].Value);
            }
            return null;
        }
        public IDictionary<int, string> GetsFromAttribute(Func<XPathHandler, XPathHandler> xpath, string attribute)
        {
            return this.GetsFromAttribute(xpath, attribute, null);
        }

        public IDictionary<int, string> GetsFromAttribute(Func<XPathHandler, XPathHandler> xpath, string attribute, Func<string, string> strHandler)
        {
            XPathHandler xpathHandler = new XPathHandler();
            var node = this._currentNode;
            var nodes = node.ChildNodes;
            if (xpath != null)
            {
                nodes = node.SelectNodes(xpath(xpathHandler).GetPath());
            }
            if (nodes != null && nodes.Count > 0)
            {
                return (from a in nodes
                        where a.Attributes.Contains(attribute)
                        select this.StrHandle(strHandler, a.Attributes[attribute].Value))
                        .Select((value, index) => new { value, index })
                        .ToDictionary(a => a.index, a => a.value);
            }
            return null;
        }

        public string Get()
        {
            return this.Get<string>(null);
        }
        public T Get<T>()
        {
            return this.Get<T>(null);
        }

        public string Get(Func<XPathHandler, XPathHandler> xpath)
        {
            return this.Get<string>(xpath, null);
        }
        public T Get<T>(Func<XPathHandler, XPathHandler> xpath)
        {
            return this.Get<T>(xpath, null);
        }

        public string Get(Func<XPathHandler, XPathHandler> xpath, Func<string, string> strHandler)
        {
            return this.Get<string>(xpath, strHandler);
        }

        public T Get<T>(Func<XPathHandler, XPathHandler> xpath, Func<string, string> strHandler)
        {
            XPathHandler xpathHandler = new XPathHandler();

            var node = this._currentNode;
            if (xpath != null)
            {
                node = node.SelectSingleNode(xpath(xpathHandler).GetPath());
            }
            if (node != null)
            {
                var result = this.StrHandle(strHandler, HtmlHandler.GetImplement(_logPrefix, node).InnerHtml).Trim();
                if (!string.IsNullOrEmpty(result))
                {
                    return (T)Convert.ChangeType(result, typeof(T)); ;
                }
            }
            return default(T);
        }

        public IDictionary<int, string> Gets()
        {
            return this.Gets(null);
        }

        public IDictionary<int, string> Gets(Func<XPathHandler, XPathHandler> xpath)
        {
            return this.Gets(xpath, null);
        }

        public IDictionary<int, string> Gets(Func<XPathHandler, XPathHandler> xpath, Func<string, string> strHandler)
        {
            XPathHandler xpathHandler = new XPathHandler();
            var node = this._currentNode;
            var nodes = node.ChildNodes;
            if (xpath != null)
            {
                nodes = node.SelectNodes(xpath(xpathHandler).GetPath());
            }
            if (nodes != null && nodes.Count > 0)
            {
                return (from a in nodes
                        select this.StrHandle(strHandler, HtmlHandler.GetImplement(_logPrefix, a).InnerHtml))
                        .Select((value, index) => new { value, index })
                        .ToDictionary(a => a.index, a => a.value);
            }
            return null;
        }

        public IDictionary<int, Dictionary<string, string>> GetsAttributes(Func<XPathHandler, XPathHandler> xpath)
        {
            return this.GetsAttributes(xpath, null);
        }

        public IDictionary<int, Dictionary<string, string>> GetsAttributes(Func<XPathHandler, XPathHandler> xpath, Func<string, string> strHandler)
        {
            XPathHandler xpathHandler = new XPathHandler();
            var node = this._currentNode;
            var nodes = node.ChildNodes;
            if (xpath != null)
            {
                nodes = node.SelectNodes(xpath(xpathHandler).GetPath());
            }
            if (nodes != null && nodes.Count > 0)
            {
                return nodes.Select((value, index) => new
                {
                    value,
                    index
                }).ToDictionary(x => x.index, x => x.value.Attributes.Select((value,index)=>new {
                    value.Name,
                    value.Value
                }).ToDictionary(y=>y.Name,y=>y.Value));
            }
            return null;
        }




        private string StrHandle(Func<string, string> strHandler, string content)
        {
            if (strHandler != null)
            {
                return strHandler(content);
            }
            return content;
        }

        public HtmlHandler Replace(string older, string newer)
        {
            if (this._currentNode != null)
            {
                this._currentNode.InnerHtml = this._currentNode.InnerHtml.Replace(older, newer);
                return this;
            }
            throw new Exception(string.Format("Can't not replace, because currentNode is null"));
        }

        public HtmlHandler ClearString()
        {
            if (this._currentNode != null)
            {
                string str = this._currentNode.InnerHtml;
                str = str.Trim();
                str = str.Replace("&nbsp;", "");
                str = str.Replace(" ", "");
                str = str.Replace("-", "");
                str = str.Replace(@"""", "");
                str = str.Replace("\r", "");
                str = str.Replace("\n", "");
                str = str.Replace("\t", "");
                str = str.Replace("，", ",");
                str = Regex.Replace(str, "<.*?>", string.Empty);
                this._currentNode.InnerHtml = str;
                return this;
            }
            throw new Exception(string.Format("Content is null"));
        }

    }
}
