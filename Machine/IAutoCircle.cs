using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    delegate void CircleStartDelegate();
    delegate void CircleStopDelegate();
    interface IAutoCircle
    {
        event CircleStartDelegate CircleStart;
        event CircleStartDelegate CircleStop;
        void Circle();
    }
}
