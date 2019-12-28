//*****************************************************************************
//*         FILE: HWGalil.cs
//*       AUTHOR: xinyiz163@163.com
//*         DATE: Dec 2019
//*  DESCRIPTION: 和Galil通信类
//*               Galil里面定义tepmTI0，tempTI1...tempOP0，tempOP1,与相应的IO
//*               通信即可，Galil IO状态更改需要定义程序用MG 反馈给GUI
//*               如：
/*                 
#DIG_OUT
IF((tempTI01<> _TI0)|(CsTpTI01<> _TI0) )
WT 5
  IF((tempTI01<> _TI0)|(CsTpTI01<> _TI0))
    tempTI01 = _TI0
    MG "_TI01=",_TI0
  ENDIF
ENDIF
IF((tempOP0<> CsTpOP0)|(CsTpOP0<> _OP0) )
    OP CsTpOP0
    MG "_OP0=",_OP0
    WT 5,0
ENDIF
JP#DIG_OUT
EN */
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Utility;
using System.Diagnostics;

namespace Machine
{
    public class HWGalil : INotifyPropertyChanged
    {
        readonly Galil.Galil galil = new Galil.Galil(); //Dimension g variable for instantiated object
        private string _ipAdress;
        private bool _isConnected;
        
        private string _message = "Default";
        private string _messageIpt = "Default Importance message";
        private string _messageRsp = "Rsp";
        List<bool> _bits=new List<bool>();
        private IO _iOOutput0Act = new IO("_OP0", IOType.Output);
        private IO _iOOutput0Exp = new IO("DigitalO0Exp", IOType.Output);
        int _ioOutput0ExpLast;//上一次设置的值，用来检测限定时间内，实际值有没有变化，如果没有需要处理
        /// <summary>
        /// 在galil负责和c#通信更新OP0的变量名
        /// </summary>
        readonly private string _csTempOP0 = "CsTpOP0";  //galil 命名长度限制
        readonly private string _temOP0 = "tempOP0";   //galil 里和galil _OP0对接的名字
        readonly private string _temTI01 = "temTI01";   //galil 里和galil _TI1*256+_TI0对接的名字
        readonly private string _temTI23 = "temTI23";   //galil 里和galil _TI3*256+_TI2对接的名字
        readonly private string _temTI45 = "temTI45";   //galil 里和galil _TI5*256+_TI4对接的名字
        readonly private string _csTpTI01 = "CsTpTI01"; //C# 里和galil _TI1*256+_TI0对接的名字
        readonly private string _csTpTI23 = "CsTpTI23"; //C# 里和galil _TI3*256+_TI2对接的名字
        readonly private string _csTpTI45 = "CsTpTI45"; //C# 里和galil _TI3*256+_TI2对接的名字

        private bool syningUpWithGalil =false;

        private IO _iOInput0 = new IO("_TI01", IOType.Input);
        private IO _iOInput1 = new IO("_TI23", IOType.Input);
        private IO _iOInput2 = new IO("_TI45", IOType.Input);

        Thread threadCicleCommu;



        /// <summary>
        /// 默认IP "192.168.1.2"
        /// </summary>
        public HWGalil()
        {
            this._ipAdress = "192.168.1.2";
            this.IsConnected = false;
            this.IOOutput0Exp.PropertyChanged += OnOutputExp0PropertyChange;           
            ;
        }
        public HWGalil(string ipAdress)
        {
            this._ipAdress = Util.CleanUpIpAddress(ipAdress);
            this.IsConnected = false;
            this.IOOutput0Exp.PropertyChanged += OnOutputExp0PropertyChange;
        }
        
        /// <summary>
        /// 所有自动获取的信息
        /// </summary>
        public string Message { get => _message; private set { this._message = value; NotifyPropertyChanged(); } }

        /// <summary>
        /// Message中重要的，需要显示的信息
        /// </summary>
        public string MessageIpt { get => _messageIpt; private set{ _messageIpt = value; NotifyPropertyChanged();} }

    /// <summary>
    /// 发送命令获得的信息
    /// </summary>
        public string MessageRsp { get => _messageRsp; private set { this._messageRsp = value; NotifyPropertyChanged(); } }
        public string IPAdress { get { return _ipAdress; } }
        /// <summary>
        /// Galili真实值，对外只有set
        /// 发送_temOP0给Galil是告诉它C#真实值已经更改了，不用再发送消息过来
        /// </summary>
        public IO IOOutput0Act
        { 
            get => _iOOutput0Act; 
            private set 
            { 
                _iOOutput0Act = value; 
                this.SendCommand(this._temOP0 + "=" + this.IOOutput0Act.IntBits.BitsToInt.ToString());
            }
        }
        /// <summary>
        /// 设置output 1-16的期望值
        /// </summary>
        public IO IOOutput0Exp { get => _iOOutput0Exp; set {  _iOOutput0Exp = value;} }
        /// <summary>
        /// input 1-16
        /// </summary>
        public IO IOInput0 { get => _iOInput0;  private set { _iOInput0 = value;  } }
        /// <summary>
        /// input 17-32
        /// </summary>
        public IO IOInput1 { get => _iOInput1; private set { _iOInput1 = value;  } }
        /// <summary>
        /// input 33-48
        /// </summary>
        public IO IOInput2 { get => _iOInput2; private set { _iOInput2 = value;  } }

        public List<bool> Bits { get => _bits; set => _bits = value; }
        public bool IsConnected { get => _isConnected; private set => _isConnected = value; }


        /// <summary>
        /// 与Galil连接通信
        /// </summary>
        /// <param name="circleCommu">true = 每隔固定时间和galil通信</param>
        /// <returns></returns>
        public string Connect(bool circleCommu = true)
        {
            string rsp = "";
            try
            {
                galil.address = this._ipAdress;
                SendCommand("CFI");   //把unsolicited Message 发给本尊
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
             
            galil.timeout_ms = 500;
            galil.onMessage += new Galil.Events_onMessageEventHandler(Galil_onMessage);
            rsp = galil.connection();
            this.IsConnected = true;
            //lock(this)
            { UpdateIOStatusInGalil();}            
            this.IOOutput0Exp.IntBits.BitsToInt = IOOutput0Act.IntBits.BitsToInt; //初始化保持原有的output值
            this.IOOutput0Act.PropertyChanged -= new PropertyChangedEventHandler(OnOutputPropertyChange);
            this.IOOutput0Exp.PropertyChanged -= new PropertyChangedEventHandler(OnOutputPropertyChange);
            this.IOOutput0Act.PropertyChanged += new PropertyChangedEventHandler(OnOutputPropertyChange);
            this.IOOutput0Exp.PropertyChanged += new PropertyChangedEventHandler(OnOutputPropertyChange);
            this.IOInput0.PropertyChanged += new PropertyChangedEventHandler(OnInputPropertyChange);
            if (circleCommu)
            {
                threadCicleCommu = new Thread(new ThreadStart(CircleCommu));
                threadCicleCommu.Name = "CircleGalilCommu";
                threadCicleCommu.IsBackground = true;
                threadCicleCommu.Priority = ThreadPriority.BelowNormal;
                threadCicleCommu.Start();
            }
            return rsp;
        }
        public string Disconnect()
        {
            this.IsConnected = false;
            if (threadCicleCommu != null && threadCicleCommu.IsAlive)
            {
                threadCicleCommu.Abort();
            }
            try
            {
                if (this.IsConnected) galil.address = "OFFLINE";
            }
            catch (Exception ex) { System.Diagnostics.Debug.Fail(ex.Message); }        
            this.IOInput0.PropertyChanged  -= OnOutputPropertyChange;
            this.IOOutput0Exp.PropertyChanged -= OnOutputPropertyChange;
            this.IOInput0.PropertyChanged -= OnInputPropertyChange;
            return "Now Offline";
        }

        /// <summary>
        /// 如果DigitalO0Act 和DigitalO0Exp不一致，则更新galil里面的输出
        /// </summary>
        private void SynAllOutputs()
        {
            if (syningUpWithGalil)
            {
                this.Message = "SynAllOutputs()正在执行";
                return;
            }
            syningUpWithGalil = true;
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (!this.IsConnected)
            {
                this.Message = "Galil 掉线了";
                return;
            }
            //UpdateIOStatusInGalil();
            if (this._iOOutput0Exp.IntBits.BitsToInt!= this._iOOutput0Act.IntBits.BitsToInt)
            {
               SendCommand(_csTempOP0 + "=" + this._iOOutput0Exp.IntBits.BitsToInt.ToString());//告诉Galil C#要求改OP0的值
                if (this._ioOutput0ExpLast==this._iOOutput0Exp.IntBits.BitsToInt) //如果上一次的Exp值和现在一样，而且Act和Exp不一样，需要计时看看一段时间后有没有一致
                {
                    //this.Message = "为什么会出现这种状况SynAllOutputs()";
                    //TO DO
                }                
            }
            //stopwatch.Stop();
            //this.Message = "It took " + stopwatch.ElapsedMilliseconds + " ms to excute SynAllOutputs()";
            syningUpWithGalil = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_cmd">发送的命令</param>
        /// <param name="updateMessage">如果需要更新Message信息来更新GUI显示，则改为true</param>
        /// <returns></returns>
        public string SendCommand(string _cmd,bool updateMessage = false)
        {
            string rspStr = "";
            //ushort rsp = 0;
            if (!this.IsConnected)
            {
                return rspStr;
            }
            try
            {
                rspStr = galil.command(_cmd);
            }
            catch (Exception ex)
            {
                rspStr = ex.Message.ToString();
            }

            //rsp = Convert.ToUInt16(rspStr.Split(new char[1] { '.' }).First().ToString());
            if (updateMessage)
            {
                this.MessageRsp = "cmd:" + _cmd + ";rsp:" + rspStr;
            }
            if (rspStr.Contains("TIMEOUT ERROR"))
            {
                this.IsConnected = false;
                Disconnect();
                Connect();
            }
            else if (rspStr.Contains("ERROR"))
            {
                this.MessageRsp = "cmd:" + _cmd + ";rsp:" + rspStr;
            }
            return rspStr;
        }

        /// <summary>
        /// 同步与IO设备的状态
        /// </summary>
        private void UpdateIOStatusInGalil()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string cmd;
            string rsp;
            //更新intputs
            cmd = "MG \"" + this.IOInput0.Name + "=\"," + "_TI0,"; //TO DO:重要！调试的机器_TI1有问题，等待复制回去： cmd = "MG \"" + this.IOInput0.Name + "=\"," + "_TI1*256+_TI0,"
            //cmd = "MG \"" + this.IOInput0.Name + "=\"," + "_TI1*256+_TI0,"
            cmd += "\"" + this.IOInput1.Name + "=\"," + "_TI3*256+_TI2,";
            cmd += "\"" + this.IOInput2.Name + "=\"," + "_TI5*256+_TI4,";
            rsp = SendCommand(cmd);
            updataIOFromGalil(this.IOInput0, rsp);
            updataIOFromGalil(this.IOInput1, rsp);
            updataIOFromGalil(this.IOInput2, rsp);

            //更新Output0
            cmd = "MG \"" + this.IOOutput0Act.Name + "=\"," + IOOutput0Act.Name + ",";
            rsp = SendCommand(cmd);
            updataIOFromGalil(this.IOOutput0Act, rsp);
            stopwatch.Stop();
            this.Message = "It took " + stopwatch.ElapsedMilliseconds + " ms to excute UpdateIOStatusInGalil()";

        }
        private void CircleCommu()
        {
            while (this.IsConnected)
            {
                SendCommand("CFI");   //把unsolicited Message 发给本尊
                if (SendCommand("MG _HX7") == "0.0000") //线程7不在执行
                {
                    this.Message = "MG _HX7 = 0.0000 正在重启XQ#DIG_OUT,7";
                    SendCommand("XQ#DIG_OUT,7");
                }
                Thread.Sleep(3000);
            }
        }      

        /// <summary>
        /// Galil自动发的消息
        /// </summary>
        /// <param name="message"></param>
        void Galil_onMessage(string message)
        { //handler for the onMessage event
            HandleOnMessage(message);
            //Thread thread = new Thread(() => HandleOnMessage(message));//创建线程
            //thread.Name = "HandleOnMessage";
            //thread.Priority = ThreadPriority.AboveNormal;
            //thread.IsBackground = true;
            //thread.Start();
        }
        private void HandleOnMessage(string message)
        {
            if (!message.EndsWith("\r\n"))
            {
                this.Message = "Not End with rn: " + message;
                return;
            }
            this.Message = message;
            if (this.Message.Contains(this.IOOutput0Act.Name) || this.Message.Contains(this.IOInput0.Name) || this.Message.Contains(this.IOInput1.Name) ||
                this.Message.Contains("?") || this.Message.Contains("ERROE"))
            {
                this.MessageIpt = this.Message;
            }
            lock (this.IOOutput0Act)
            {
                ///信息里包含_OP0值
                if (this.Message.Contains(this.IOOutput0Act.Name))
                {
                    updataIOFromGalil(this.IOOutput0Act, this.Message);
                }
            }
            lock (this.IOInput0)
            {
                if (this.Message.Contains(this.IOInput0.Name))
                {
                    updataIOFromGalil(this.IOInput0, this.Message);                   
                }
            }
            lock (this.IOInput1)
            {
                if (this.Message.Contains(this.IOInput1.Name))
                {
                    updataIOFromGalil(this.IOInput1, this.Message);
                }
            }
            lock (this.IOInput2)
            {
                if (this.Message.Contains(this.IOInput2.Name))
                {
                    updataIOFromGalil(this.IOInput2, this.Message);
                }
            }
        }

        /// <summary>
        /// 更新IO Act
        /// </summary>
        /// <param name="iO"></param>
        /// <param name="msg"></param>
        /// <param name="iOName">Galil返回来的名字，iOName=="" 会引用iO.Name</param>
        private void updataIOFromGalil(IO iO, string msg, string iOName = "")
        {
            if (iOName == "")
            {
                iOName = iO.Name;
            }
            string temp = ExtractValueFrmMsg(msg, iO.Name);
            if (temp != "")
            {
                //iO.IntBits = new Util.IntBits(ConvertMsgToInt(temp, iO.IntBits.BitsToInt));                
                iO.IntBits.BitsToInt = ConvertMsgToInt(temp, iO.IntBits.BitsToInt);//不能新建，否则之前引用的会不再更新

                if (iO.Name == this.IOOutput0Act.Name)
                {
                    this.SendCommand(this._temOP0 + "=" + this.IOOutput0Act.IntBits.BitsToInt.ToString()); //告诉Galil C#已经读到galil的output值
                }
                else if (iO.Name == this.IOInput0.Name)
                {
                    SendCommand(_csTpTI01 + "=" + this.IOInput0.IntBits.BitsToInt.ToString());   // 告诉Galil C#已经读到galil的intput0值
                }
                else if (iO.Name == this.IOInput0.Name)
                {
                    SendCommand(_csTpTI23 + "=" + this.IOInput1.IntBits.BitsToInt.ToString());  // 告诉Galil C#已经读到galil的intput1值
                }
                else if (iO.Name == this.IOInput2.Name)
                {
                    SendCommand(_csTpTI45 + "=" + this.IOInput2.IntBits.BitsToInt.ToString());   // 告诉Galil C#已经读到galil的intput2值
                }
            }
        }

        /// <summary>
        /// 从Galil 原始信息提取iOName=xx.xxx 如果信息不全，则返回""
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="iOName"></param>
        /// <returns></returns>
        private string ExtractValueFrmMsg(string msg, string iOName)
        {
            string result = "";
            string temp;
            iOName += "=";
            if (msg.Contains(iOName))
            {
                try
                {
                    temp = System.Text.RegularExpressions.Regex.Split(msg, iOName).Last();
                    temp = temp.Split(new char[1] { '.' }).First();
                    result = temp;
                }
                catch { }
            }
            return result;
        }
        private int ConvertMsgToInt(string msg,int origonValue)
        {
            int result = origonValue;  //默认值
            try
            {
                if (msg != "")
                {
                    result = Convert.ToInt32(msg);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));          
            
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnOutputPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SynAllOutputs();
        }
        private void OnInputPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.SendCommand(this._csTpTI01 + "=" + this.IOInput0.IntBits.BitsToInt.ToString());
            //Thread thread = new Thread(new ThreadStart(UpdateIOStatusInGalil));//创建线程
            //Thread thread = new Thread(() => { this.SendCommand(this._csTpTI01 + "=" + this.IOInput0.IntBits.BitsToInt.ToString()); });
            //thread.Name = "UpdateIOStatusInGalil";
            //thread.Priority = ThreadPriority.AboveNormal;
            //thread.IsBackground = true;
            //thread.Start();
        }
        private void OnOutputExp0PropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //_ioOutput0ExpLast = _iOOutput0Exp.IntBits.BitsToInt;
        }
    }
}
