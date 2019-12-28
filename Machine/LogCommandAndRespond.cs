using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    public enum LogCmdRspType
    { 
        Command,
        Respond,
        None
    }
    public class LogCommandAndRespond
    {
        public LogCommandAndRespond(LogCmdRspType cmdRspType, string infomation)
        {
            Time = DateTime.Now;
            CmdRspType = cmdRspType;
            Infomation = infomation ?? throw new ArgumentNullException(nameof(infomation));
        }

        public DateTime Time { get; private set; }
        public LogCmdRspType CmdRspType { get; private set; }
        public string Infomation { get; private set; }
    }
}
