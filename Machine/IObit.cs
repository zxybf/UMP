using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    public class IObit
    {
        int _id;
        bool _status;
        IO iOSource;
        private ForceStatusSource forceStatusSourceProperty;

        /// <summary>
        /// IO连线
        /// </summary>
        /// <param name="ioSource">IO bit连接到的IO硬件</param>
        /// <param name="id">连接IO硬件的哪一位</param>
        public IObit(IO ioSource, int id)
        {
            this.iOSource = ioSource;
            Id = id;
            Status = ioSource.IntBits[Id];            
            this.ForceStatusSourceProperty = ForceStatusSource.ConnectHardWare;
        }
    /// <summary>
    /// Set bit
    /// </summary>
    public void SB()
        {
            this.iOSource.IntBits[Id] = true;
        }
        /// <summary>
        /// Clear bit
        /// </summary>
        public void CB()
        {
            this.iOSource.IntBits[Id] = false;
        }
        public enum ForceStatusSource
        {
            ConnectHardWare,
            ForceSetTrue,
            FoceSetFalse,
        }
        /// <summary>
        /// 从1开始到16，如果超出则设置为1
        /// </summary>
        public int Id { get => _id; set { if (value < 1 || value > 17) _id = 0; else _id = value - 1; } } //value-1是由于IntBit是从0开始的
        public bool Status { get => _status; set => _status = value; }
        public bool StatusIntBitID { get { return this.IOSource.IntBits[Id]; } }
        public IO IOSource { get => iOSource; }

        /// <summary>
        /// 用来强制某一位为true，false，或者是跟随this.iOSource.IntBits[Id]
        /// </summary>
        public ForceStatusSource ForceStatusSourceProperty 
        { 
            get => forceStatusSourceProperty;
            set
            {
                forceStatusSourceProperty = value;
                switch (this.ForceStatusSourceProperty)
                {
                    case ForceStatusSource.ConnectHardWare:
                        this.IOSource.PropertyChanged -= OnPropertyChange;
                        this.IOSource.PropertyChanged += OnPropertyChange;
                        this.Status = this.iOSource.IntBits[Id];                      
                        break;
                    case ForceStatusSource.ForceSetTrue:
                        this.IOSource.PropertyChanged -= OnPropertyChange;
                        this.Status = true;
                        break;
                    case ForceStatusSource.FoceSetFalse:
                        this.IOSource.PropertyChanged -= OnPropertyChange;
                        this.Status = false;
                        break;
                    default:
                        this.IOSource.PropertyChanged -= OnPropertyChange;
                        this.IOSource.PropertyChanged += OnPropertyChange;
                        this.Status = this.iOSource.IntBits[Id];

                        break;
                }
            }
        }
        private void OnPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Status = this.iOSource.IntBits[Id];  //TO DO:目前每个bit更新都会使得所有bit都更新，效率待提高
        }

    }
}
