using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    public enum MessageType
    {
        Worning,
        Error
    }
    public class Message
    {
        public Message(MessageType messageType, string divce, string additionInfo)
        {
            TriggerTime = DateTime.UtcNow;
            MessageType = messageType;
            Divce = divce ?? throw new ArgumentNullException(nameof(divce));
            AdditionInfo = additionInfo ?? throw new ArgumentNullException(nameof(additionInfo));
        }
        public DateTime TriggerTime { get; private set; } 
        public MessageType MessageType { get; private set; }
        public string Divce { get; private set; }
        public string AdditionInfo { get; private set; }
    }
}
