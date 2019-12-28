//*****************************************************************************
//*         FILE: IO.cs
//*       AUTHOR: Xinyi
//*         DATE: Dec 2019
//*  DESCRIPTION: IO类，所有IO设备都应该有IO类，有 INotifyPropertyChanged 接口，
//*               可以通知IO状态变更
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
    public enum IOType
    {
        Input,
        Output
    }
    public class IO : INotifyPropertyChanged
    {
        private string _name="UNKNOW";
        private bool _isFirstUpate; //用于通知客户端BIT01.。。。Bit16已更改
        private IOType _iOType;
        private Util.IntBits _intBits = new Util.IntBits(0);
        //private Util.IntBits _intOutputBits = new Util.IntBits();
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get => _name; private set { _name = value; NotifyPropertyChanged(); } }
        public IOType IOType { get => _iOType; private set { _iOType = value; NotifyPropertyChanged(); } }

        /// <summary>
        /// 可以通过IntBits更新IO状态,此处IO都是public的，需要在写IO设备时注意input的权限应为private； TO do： public不是最好的选择
        /// </summary>
        public Util.IntBits IntBits
        {
            get => _intBits;
            set
            {
                if (_intBits.GetBits()!=value.GetBits())
                {
                    _intBits = value;                    
                    NotifyPropertyChanged();
                }                
            }
        }
        #region  Bit01-16 difition
        public bool Bit01
        {
            get => _intBits[0];
            set { if (_intBits[0] ^ value) _intBits[0] = value; }
        }
        public bool Bit02
        {
            get => _intBits[1];
            set { if (_intBits[1] ^ value) _intBits[1] = value; }
        }
        public bool Bit03
        {
            get => _intBits[2];
            set { if (_intBits[2] ^ value) _intBits[2] = value; }
        }
        public bool Bit04
        {
            get => _intBits[3];
            set { if (_intBits[3] ^ value) _intBits[3] = value; }
        }
        public bool Bit05
        {
            get => _intBits[4];
            set { if (_intBits[4] ^ value) _intBits[4] = value; }
        }
        public bool Bit06
        {
            get => _intBits[5];
            set { if (_intBits[5] ^ value) _intBits[5] = value; }
        }
        public bool Bit07
        {
            get => _intBits[6];
            set { if (_intBits[6] ^ value) _intBits[6] = value; }
        }
        public bool Bit08
        {
            get => _intBits[7];
            set { if (_intBits[7] ^ value) _intBits[7] = value; }
        }
        public bool Bit09
        {
            get => _intBits[8];
            set { if (_intBits[8] ^ value) _intBits[8] = value; }
        }
        public bool Bit10
        {
            get => _intBits[9];
            set { if (_intBits[9] ^ value) _intBits[9] = value; }
        }
        public bool Bit11
        {
            get => _intBits[10];
            set { if (_intBits[10] ^ value) _intBits[10] = value; }
        }
        public bool Bit12
        {
            get => _intBits[11];
            set { if (_intBits[11] ^ value) _intBits[11] = value; }
        }
        public bool Bit13
        {
            get => _intBits[12];
            set { if (_intBits[12] ^ value) _intBits[12] = value; }
        }
        public bool Bit14
        {
            get => _intBits[13];
            set { if (_intBits[13] ^ value) _intBits[13] = value; }
        }
        public bool Bit15
        {
            get => _intBits[14];
            set { if (_intBits[14] ^ value) _intBits[14] = value; }
        }
        public bool Bit16
        {
            get => _intBits[15];
            set { if (_intBits[15] ^ value) _intBits[15] = value; }
        }
        #endregion

        /// <summary>
        /// 需要更新客户端的Bit01...Bit16状态，设置位true则更新一次
        /// </summary>
        //public bool IsFirstUpate { get => _isFirstUpate; set => _isFirstUpate = value; }
  

        public IO(string name, IOType iOType)
        {
            this.Name = name;
            this.IOType = iOType;
            this.IntBits.PropertyChanged += IntBits_PropertyChanged;
            //IsFirstUpate = true;
        }

        public void IntBits_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
            //if (this.IOType == IOType.Output) return;
            if (!e.PropertyName.StartsWith("Bit"))
                PropertyChangedInvokeAllBit();
        }
        private void PropertyChangedInvokeAllBit()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit01"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit02"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit03"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit04"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit05"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit06"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit07"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit08"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit09"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit10"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit11"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit12"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit13"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit14"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit15"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Bit16"));
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
