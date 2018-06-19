using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Enum
{
    public enum CrawlerEngingEnum
    {
        HttpRequestPost,
        HttpRequest,
        Phantom,
        Chrome
    }

    public enum TagEnum
    {
        Div,
        Span,
        Table,
        Tbody,
        Tr,
        Td,
        Input,
        B,
        A,
        Ul,
        Li,
        H4,
        Text,
        Img,
        I,
        Item,
        P,
        H3
    }
}
