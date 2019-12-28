using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Metrology;

namespace Machine
{
    delegate void LostPartDelegate();
    public enum PartHandlerStatus
    {
        Initialization = 0,
        Idle,
        WaitingForParts,
        LoadPart,  //从上一个PartHanlder里拿物料
        PartLocked,
        UnloadPart,
        Erroer
    }
    public interface  IPartHandler
    {        
        Part PartRight { get; set; }
        Part PartLeft { get; set; }
        IPartHandler PartSource { get; set; }
        IPartHandler PartDestination { get; set; }
        /// <summary>
        /// 传感器感应到的状态
        /// </summary>
        bool HasPartsSensor { get; }
        /// <summary>
        /// 应该有料=true
        /// </summary>
        bool HasPartsExpect { get; }

        bool ReadyForParts();

        //void PlaceComplete(PartData _part1, PartData _part2);

        void PickComplete();

        bool ReadyForPick();

        void ReleaseParts(bool _releaseVacuum);

        bool Idle { get; }
        PartHandlerStatus PartHandlerStatus { get; set; }

        void GrabParts(Part partRight,Part partLeft);
        Part HandOverParts();

    }
}
