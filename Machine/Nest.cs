using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Panuon.UI.Silver;

namespace Machine
{
    public enum NestType
    {
        T1WithJHook12,
        T1WithJHook34,
        T1KDeath,
        UMNest12,
        UMNest34,
        UMWithRotator12,
        T3NVaccum12,
        T3NVaccum34,
    }
    public  class Nest : IPartHandler, INotifyPropertyChanged
    {
        IObit partSence;
        IObit partSence34; //同时夹的时候用到
        IObit vaccumOrZClamp;        
        IObit Finger;  //T3只有longfinger
        IObit Finger34;  //同时夹的时候用到
        IObit nestPuff;
        IObit umRotatorVac;  //Z模组上的rotator真空
        IObit partSenceumRotatorc;  //Z模组上的rotator真空
        IObit umRotatorPuff;  //Z模组上的rotator喷气
        Part part;
        private Message message;
        public event PropertyChangedEventHandler PropertyChanged;
        public Message Message { get => message; set { message = value; NotifyPropertyChanged(); } }
        public Nest(NestType nestType)
        {
            this.NestType = nestType;
            this.Idle = true;
            //this.IObits = IOInterface.GetNestBits(nestType);
            switch (this.NestType)   //根据Nest来设置IO
            {
                case NestType.T1WithJHook12:
                    this.partSence = IOInterface.IT1NestPartPresent12;
                    this.vaccumOrZClamp = IOInterface.T1NestExtendZClamps;
                    this.Finger = IOInterface.T1Nest12Finger;
                    this.partSence = IOInterface.IT1NestPartPresent34;
                    this.Finger34 = IOInterface.T1Nest34Finger;
                    break;
                case NestType.T1WithJHook34:
                    this.partSence = IOInterface.IT1NestPartPresent34;
                    this.vaccumOrZClamp = IOInterface.T1NestExtendZClamps;
                    this.Finger = IOInterface.T1Nest34Finger;
                    this.Finger34 = IOInterface.T1Nest34Finger;
                    break;
                case NestType.T1KDeath:
                    break;
                case NestType.UMNest12:
                    break;
                case NestType.UMNest34:
                    break;
                case NestType.UMWithRotator12:
                    break;
                case NestType.T3NVaccum12:
                    this.partSence = IOInterface.IT3NestVac12OK;
                    this.vaccumOrZClamp = IOInterface.T3NestVacLeft;
                    this.Finger = IOInterface.T3NestFinger;
                    this.nestPuff = IOInterface.T3NestPufLeft;
                    break;
                case NestType.T3NVaccum34:
                    this.partSence = IOInterface.IT3NestVac34OK;
                    this.vaccumOrZClamp = IOInterface.T3NestVacRight;
                    this.Finger = IOInterface.T3NestFinger;
                    this.nestPuff = IOInterface.T3NestPufLeft;
                    break;
                default:
                    break;
            }
            this.NestIObits = new List<IObit>(7);
            if (partSence != null)      this.NestIObits.Add(partSence);
            if (vaccumOrZClamp != null) this.NestIObits.Add(vaccumOrZClamp);
            if (Finger != null)         this.NestIObits.Add(Finger);
            if (nestPuff != null)       this.NestIObits.Add(nestPuff);
            if (umRotatorVac != null)   this.NestIObits.Add(umRotatorVac);
            if (umRotatorPuff != null)  this.NestIObits.Add(umRotatorPuff);            
            Initialization();
        }
        public NestType NestType { get; private set; }
        public List<IObit> NestIObits { get; private set; }
        /// <summary>
        /// 初始要检查有没有物料，只在构造的时候调用
        /// </summary>
        private void Initialization()
        {
            this.HasPartsExpect =  this.partSence.Status;  //万一原来有料，IO动作后无料了，能处理异常
            switch (this.NestType)
            {
                case NestType.T1WithJHook12:
                case NestType.T1WithJHook34:
                    break;
                case NestType.T1KDeath:
                    break;
                case NestType.UMNest12:
                case NestType.UMNest34:
                    break;
                case NestType.UMWithRotator12:
                    break;
                case NestType.T3NVaccum12:
                case NestType.T3NVaccum34:
                    break;
                default:
                    break;
            }
            CheckIfHasPart();
        }
        private void CheckIfHasPart()
        {
            Thread thread = new Thread(() =>
            {
                if (NestIsSnug() && this.HasPartsSensor) //如果有物料，而且也是夹紧的状态，那就不用在检查了
                { this.HasPartsExpect = this.HasPartsSensor; return; }
                NestUnSnug();
                NestSnug();

            });
            thread.Name = "CheckIfHasPart";
            thread.Priority = ThreadPriority.AboveNormal;
            thread.IsBackground = true;
            thread.Start();
        }
        private void InitAllIO()
        {
            if (NestIObits != null && NestIObits.Count > 2)
            {
                for (int i = 0; i < NestIObits.Count - 1; i++)
                {
                    for (int j = i + 1; j < NestIObits.Count; j++)
                    {
                        if (NestIObits[i].Id == NestIObits[j].Id && NestIObits[i].IOSource == NestIObits[j].IOSource)
                        {
                            System.Diagnostics.Debug.WriteLine(NestIObits[i].IOSource.ToString() + "ID:" + NestIObits[i].Id.ToString() + "Defined more than 1");
                        }
                    }

                }
            }
        }
        public bool NestIsSnug()
        {
            switch (this.NestType)
            {
                case NestType.T1WithJHook12:
                case NestType.T1WithJHook34:
                case NestType.UMNest12:
                case NestType.UMNest34:
                case NestType.T3NVaccum12:
                case NestType.T3NVaccum34:
                    return !Finger.Status && !vaccumOrZClamp.Status;
                case NestType.UMWithRotator12:
                    return !Finger.Status && vaccumOrZClamp.Status && !umRotatorVac.Status && !partSenceumRotatorc.Status;
                default:
                    break;
            }
            return false;
        }
        public bool NestIsUnSnug()
        {
            switch (this.NestType)
            {
                case NestType.T1WithJHook12:
                case NestType.T1WithJHook34:
                case NestType.UMNest12:
                case NestType.UMNest34:
                case NestType.T3NVaccum12:
                case NestType.T3NVaccum34:
                    return Finger.Status && vaccumOrZClamp.Status;
                case NestType.UMWithRotator12:
                    return Finger.Status && !vaccumOrZClamp.Status && !umRotatorVac.Status && !partSenceumRotatorc.Status;
                default:
                    break;
            }
            return false;
        }
        /// <summary>
        /// 夹紧物料
        /// </summary>
        /// <param name="ativeNest1234">是否同时夹紧，默认是</param>
        public void NestSnug(bool ativeNest1234 = true)
        {
            if (!this.Idle)
            {
                this.Message = new Message(MessageType.Worning, this.NestType.ToString(), "正在忙，请稍后再试");
                return;
            }
            this.Idle = false;
            if (NestIsSnug()) NestUnSnug();
            if (this.HasPartsExpect && this.nestPuff != null)  //如果有料要抓，先打开喷气
            {
                this.nestPuff.SB();
                Thread.Sleep(1000);
            }
            CloseFinger(ativeNest1234);
            Thread.Sleep(50);
            if (this.nestPuff != null) this.nestPuff.CB();
            if (this.NestType == NestType.T1WithJHook12 || this.NestType == NestType.T1WithJHook34)
            {
                this.vaccumOrZClamp.CB(); //T1的是钩子，CB才是JHook下去压料
            }
            else this.vaccumOrZClamp.SB();
            Thread.Sleep(50);
            if(this.HasPartsExpect!=this.HasPartsSensor)
            {
                if (this.HasPartsSensor) this.HasPartsExpect = this.HasPartsSensor;
                else
                {                    //TO DO: 应该有物料，但是没有感应到                    
                    this.Message = new Message(MessageType.Worning, this.NestType.ToString(), "没有感应到物料，请检查Nest里有没有物料，如果没有请清除状态");
                }
            }
            this.Idle = true;
        }
        public void NestUnSnug(bool ativeNest1234 = true)
        {
            if (!this.Idle)
            {
                this.Message = new Message(MessageType.Worning, this.NestType.ToString(), "正在忙，请稍后再试");
                return;
            }
            this.Idle = false;
            if (NestIsUnSnug()) NestSnug();
            if (this.NestType == NestType.T1WithJHook12 || this.NestType == NestType.T1WithJHook34) this.vaccumOrZClamp.SB(); //T1的是钩子，CB才是JHook下去压料
            else this.vaccumOrZClamp.CB();
            if (this.nestPuff != null) this.nestPuff.CB();
            Thread.Sleep(50);            
            OpenFinger(ativeNest1234);
            Thread.Sleep(50);
            this.Idle = true;
        }
        private void CloseFinger(bool ativeNest1234)
        {
            this.Finger.CB();
            if(ativeNest1234) this.Finger34.CB();
        }
        private void OpenFinger(bool ativeNest1234)
        {
            this.Finger.SB();
            if (ativeNest1234) this.Finger34.SB();
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (HWDevces.Messages == null) HWDevces.Messages = new List<Message>();
            HWDevces.Messages.Add(this.Message);
        }
        #region IPartHandler Interface
        public IPartHandler PartSource { get; set; }
        public IPartHandler PartDestination { get; set; }
        public bool HasPartsSensor { get { return partSence.Status; } }
        public bool HasPartsExpect { get; private set; }
        public PartHandlerStatus PartHandlerStatus { get; set; }
        public bool ReadyForPick() { return ReadyForParts(); }       
        public bool Idle { get; private set; }
        public Part PartRight { get => part; set => part = value; }
        public Part PartLeft { get => part; set => part = value; }
       

        public void GrabParts(Part partRight, Part partLeft) {this.PartRight = partRight;this.PartLeft = PartLeft; }
        public void ReleaseParts(bool _releaseVacuum) {; }
        public Part HandOverParts() { return this.PartRight; }      
        public void PickComplete()
        {
            //return this.partSence.Status && this.HasPartsExpect && NestIsSnug;
        }
        public bool ReadyForParts()
        {
            return !this.partSence.Status && !HasPartsExpect && NestIsUnSnug();
        }
        #endregion
    }
}
