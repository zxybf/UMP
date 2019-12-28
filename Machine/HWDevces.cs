//*****************************************************************************
//*         FILE: HWDevces.cs
//*       AUTHOR: xinyiz163@163.com
//*         DATE: Dec 2019
//*  DESCRIPTION: 在此添加所有的IO控制设备如Galil DMC RIO, PLC... 
//*****************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Machine
{
    static public class HWDevces
    {
        static public HWGalil HWGalil1 = new HWGalil();
        static public HWGalil HWGalil2 = new HWGalil();
        static public HWGalil HWGalilRIO = new HWGalil();
        static public Nest Nest12 = new Nest(NestType.T1WithJHook12);
        static public Nest Nest34 = new Nest(NestType.T1WithJHook34);
        static public List<Message> Messages { get; set; }
    }
}
