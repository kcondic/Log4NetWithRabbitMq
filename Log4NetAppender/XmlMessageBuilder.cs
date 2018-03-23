﻿using System.IO;
using System.Text;
using log4net.Core;
using log4net.Layout;

namespace Log4NetAppender
{
    public class XmlMessageBuilder
    {
        private XmlLayout _xmlLayout;
        private Encoding _encoding;

        public void ActivateOptions()
        {
            _xmlLayout = new XmlLayout { Prefix = null };
            _xmlLayout.ActivateOptions();

            _encoding = Encoding.UTF8;
        }

        public string ContentEncoding
        {
            get { return _encoding.WebName; }
        }

        public string ContentType
        {
            get { return _xmlLayout.ContentType; }
        }

        public byte[] Build(LoggingEvent[] logs)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(@"<?xml version=""1.0"" encoding=""{0}""?><events version=""1.2"" xmlns=""http://logging.apache.org/log4net/schemas/log4net-events-1.2"">", ContentEncoding);
            using (var sr = new StringWriter(sb))
            {
                foreach (LoggingEvent log in logs)
                {
                    _xmlLayout.Format(sr, log);
                }
            }
            sb.Append("</events>");
            return _encoding.GetBytes(sb.ToString());
        }
    }
}
