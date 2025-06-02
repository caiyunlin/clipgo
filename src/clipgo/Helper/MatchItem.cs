using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace clipgo
{
    public enum MatchType
    {
        Text,
        Image,
        Json,
        Xml
    }

    public class MatchItem
    {
        public int Index;
        public MatchType Type;
        public string Params;
        public XmlNode Node;
        public MatchItem(XmlNode node)
        {
            Node = node;

            string type = node.Attributes["type"].Value;

            if (type == "text")
            {
                Type = MatchType.Text;
            }
            else if (type == "image")
            {
                Type = MatchType.Image;
            }
            else if (type == "json")
            {
                Type = MatchType.Json;
            }
            else if (type == "xml")
            {
                Type = MatchType.Xml;
            }

            if (node.Attributes["params"] != null)
            {
                Params = node.Attributes["params"].Value;
            }
        }
    }
}
