
using Felix.Bet365.NETCore.Crawler.Enum;
using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Handler
{
    public class XPathHandler
    {
        private string _xpath;
        public string GetPath()
        {
            return this._xpath;
        }

        public XPathHandler Custom(string xpath)
        {
            this._xpath = this._xpath + xpath;
            return this;
        }

        public XPathHandler Find(TagEnum tag, params string[] attributes)
        {
            return this.Find(tag, 0, attributes);
        }
        public XPathHandler Find(TagEnum tag, int seq, params string[] attributes)
        {
            return this.Find(tag.ToString(), seq, attributes);
        }
        public XPathHandler Find(string tag, params string[] attributes)
        {
            return this.Find(tag, 0, attributes);
        }
        public XPathHandler Find(string tag, int seq, params string[] attributes)
        {
            var attributesStr = string.Join("", from a in attributes
                                                select "[" + a + "]");
            var seqStr = seq > 0 ? string.Format("[{0}]", seq + 1) : "";
            var xpath = string.Format("//{0}{1}{2}", tag.ToLower(), attributesStr, seqStr);

            this._xpath = this._xpath + xpath;

            return this;
        }

        public XPathHandler Children(TagEnum tag, params string[] attributes)
        {
            return this.Children(tag, 0, attributes);
        }
        public XPathHandler Children(TagEnum tag, int seq, params string[] attributes)
        {
            return this.Children(tag.ToString(), seq, attributes);
        }
        public XPathHandler Children(string tag, params string[] attributes)
        {
            return this.Children(tag, 0, attributes);
        }
        public XPathHandler Children(string tag, int seq, params string[] attributes)
        {
            var attributesStr = string.Join("", from a in attributes
                                                select "[" + a + "]");
            var seqStr = seq > 0 ? string.Format("[{0}]", seq + 1) : "";
            var xpath = string.Format("/{0}{1}{2}", tag.ToLower(), attributesStr, seqStr);

            this._xpath = this._xpath + xpath;
            return this;
        }

        public XPathHandler Parent()
        {
            return this.Parent(1);
        }
        public XPathHandler Parent(int level)
        {
            var xpath = "";
            for (int i = 0; i < level; i++)
            {
                xpath = xpath + "/..";
            }
            this._xpath = this._xpath + xpath;

            return this;
        }
    }
}
