using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    public class Part
    {
        string _result;

        public Part(string result) => Result = result ?? throw new ArgumentNullException(nameof(result));

        public string Result { get => _result; set => _result = value; }
    }
}
