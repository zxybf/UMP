using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Panuon.UI.Silver;

namespace Machine
{
    public enum AxisID
    {
        A, B, C, D, E
    }
    public class AxisSimulator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private AxisID axisID;
        private float speed;   //单位是mm/s
        private int maxSpeed;
        private float positionLimitNegative;
        private float positionLimitPositive;
        private float positionCurrent;
        private float positionDestination;
        private bool idle;

        public AxisSimulator(AxisID axisID, float _positionLimitPositive = 455f)
        {
            AxisID = axisID;
            Idle = true;
            positionLimitNegative = -5f;
            this.positionLimitPositive = _positionLimitPositive;
            speed = 1f;
            maxSpeed = 1000;
            this.PositionCurrent = 0;
            this.PositionDestination = 0;
        }

        public AxisID AxisID { get => axisID; private set => axisID = value; }
        public bool Idle
        {
            get => idle; 
            private set
            {
                if (idle != value) { idle = value; NotifyPropertyChanged(); }  //通知是否在运动状态
            }
        }

        //单位是mm/s
        public float Speed
        {
            get => speed;
            set
            {
                if (value > maxSpeed) speed = maxSpeed;
                else if (value < 0) speed = 0f;
                else speed = value;
            }
        }
        public float PositionCurrent
        {
            get => positionCurrent;
            set
            {
                positionCurrent = value;
                NotifyPropertyChanged();   //通知现在的位置信息
                if (value > positionLimitPositive || value < positionLimitNegative)
                    ;//Notice.Show("Axis" + this.AxisID.ToString() + "is out of range", "Notice", 5);
            }
        }

        public float PositionDestination 
        { 
            get => positionDestination;
            private set
            {
                if (value > positionLimitPositive || value < positionLimitNegative)
                    Notice.Show("Axis" + this.AxisID.ToString() + "is out of range , will not move", "Notice", 5);
                else positionDestination = value;
            }
        }

        public void MoveAbsolute(float absolutePistion,float _speed)
        {
            if (!Idle) Notice.Show("Axis" + AxisID.ToString() + "is moving", "Notice", 5);
            else
            {
                this.Speed = _speed;
                this.PositionDestination = absolutePistion;
                Thread thread = new Thread(() => { Move(); }) ; 
                thread.Name = "AxisSimulatorMoveAbsolute";
                thread.Priority = ThreadPriority.Highest;
                thread.IsBackground = true;
                thread.Start();
            }
        }
        /// <summary>
        /// 相对现在的位置，慢速（50mm/s）移动
        /// </summary>
        /// <param name="referencePistion">正负表示方向</param>
        public void JogReference(float referencePistion)
        {
            if (!Idle) Notice.Show("Axis" + AxisID.ToString() + "is moving", "Notice", 5);
            else
            {
                this.Speed = Math.Abs(referencePistion) < 0.1f ? 0.001f * 1000f / 10f : 100;
                this.PositionDestination = this.PositionCurrent + referencePistion;
                Thread thread = new Thread(() => { Move(); });
                thread.Name = "AxisSimulatorJogReference";
                thread.Priority = ThreadPriority.BelowNormal;
                thread.IsBackground = true;
                thread.Start();
            }
        }
        private void Move()
        {
            this.Idle = false;
            int sleepTime = 10;
            if (PositionCurrent > PositionDestination)
            {
                while (PositionCurrent > PositionDestination + this.Speed * sleepTime / 1000)
                {
                    Thread.Sleep(sleepTime);
                    PositionCurrent -= this.Speed * sleepTime / 1000;
                }
                while (PositionCurrent> PositionDestination)
                {
                    if (PositionCurrent - PositionDestination > 0.1f)
                    {
                        Thread.Sleep(1);
                        PositionCurrent -= 0.099f;
                        continue;
                    }
                    Thread.Sleep(1);
                    PositionCurrent -= 0.001f;
                }

            }
            else if (PositionCurrent < PositionDestination)
            {
                while (PositionCurrent + this.Speed * sleepTime / 1000 < PositionDestination)
                {
                    Thread.Sleep(sleepTime);
                    PositionCurrent += this.Speed * sleepTime / 1000;
                }
                while (PositionCurrent < PositionDestination)
                {
                    if(PositionDestination-PositionCurrent>0.1f)
                    {
                        Thread.Sleep(1);
                        PositionCurrent += 0.099f;
                        continue;
                    }
                    Thread.Sleep(1);
                    PositionCurrent += 0.001f;
                }
            }
            this.Idle = true;
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
