//*****************************************************************************
//*         FILE: IOInterface.cs
//*       AUTHOR: xinyiz163@163.com
//*         DATE: Dec 2019
//*  DESCRIPTION: 所有的IO在这里定义，定义的时候由于可能所有的项目IO都在这里，
//*               可以定义重复的IObit硬件端，但是实例的时候就不能重复
//*               比如：定义了一个Nest类，Nest类定义了NestVac12OK端口为Galil ouput8
//*                     以后不能再次定义Galil1的端口8了
//*****************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
      static public class IOInterface
    {
        /// <summary>
        /// Mch = Machine
        /// </summary>
        #region Galil1 IOs
        //定义HWGalil1 input 1-8
        static public IObit IMchSystemAirOK          = new IObit(HWDevces.HWGalil1.IOInput0, 1);
        static public IObit IMchFrontDoorSafe1       = new IObit(HWDevces.HWGalil1.IOInput0, 2);
        static public IObit IMchFrontDoorSafe2       = new IObit(HWDevces.HWGalil1.IOInput0, 3);
        static public IObit IMchFrontDoorClosed      = new IObit(HWDevces.HWGalil1.IOInput0, 4);
        static public IObit IMchRearDoorClosed       = new IObit(HWDevces.HWGalil1.IOInput0, 5);        

        //static public IObit IMchRearDoorSafe1        = new IObit(HWDevces.HWGalil1.IOInput0, 5);
        //static public IObit IMchRearDoorSafe2        = new IObit(HWDevces.HWGalil1.IOInput0, 6);
        //static public IObit IMchRearDoorClosed1      = new IObit(HWDevces.HWGalil1.IOInput0, 7);
        static public IObit IMchEstopButtonOK        = new IObit(HWDevces.HWGalil1.IOInput0, 8);

        //定义HWGalil1 input 9-16
        //T3
        static public IObit SpareInput1X06           = new IObit(HWDevces.HWGalil1.IOInput0, 6);
        static public IObit T3XBCR1OK                = new IObit(HWDevces.HWGalil1.IOInput0, 7);
        static public IObit IT3NestVac12OK           = new IObit(HWDevces.HWGalil1.IOInput0, 9);
        static public IObit IT3NestVac34OK           = new IObit(HWDevces.HWGalil1.IOInput0, 10);
        static public IObit IT3LeftXferVacOK        = new IObit(HWDevces.HWGalil1.IOInput0, 11);
        static public IObit IT3RightXferVacOK       = new IObit(HWDevces.HWGalil1.IOInput0, 12);
        //T1N
        static public IObit IT1SMGripperRetract     = new IObit(HWDevces.HWGalil1.IOInput0, 6);
        static public IObit T1lBCR1OK               = new IObit(HWDevces.HWGalil1.IOInput0, 7);
        static public IObit IT1NestPartPresent12    = new IObit(HWDevces.HWGalil1.IOInput0, 9);
        static public IObit IT1NestPartPresent34    = new IObit(HWDevces.HWGalil1.IOInput0, 10);
        static public IObit IT1SMACPartPresent      = new IObit(HWDevces.HWGalil1.IOInput0, 11);
        static public IObit IT1SMACGripperExtended  = new IObit(HWDevces.HWGalil1.IOInput0, 12);

        //UM
        static public IObit UMPartPresent1          = new IObit(HWDevces.HWGalil1.IOInput0, 9);
        static public IObit UMPartPresent2          = new IObit(HWDevces.HWGalil1.IOInput0, 10);
        static public IObit UMPartPresent3          = new IObit(HWDevces.HWGalil1.IOInput0, 11);
        static public IObit UMPartPresent4          = new IObit(HWDevces.HWGalil1.IOInput0, 12);
   
        //All modules
        static public IObit MchGearing              = new IObit(HWDevces.HWGalil1.IOInput0, 13);
        static public IObit MchEstopRelayOK         = new IObit(HWDevces.HWGalil1.IOInput0, 14);
        static public IObit MchRobotLoadEStopOK     = new IObit(HWDevces.HWGalil1.IOInput0, 15);
        static public IObit MchRobotUnloadEStopOK   = new IObit(HWDevces.HWGalil1.IOInput0, 16);


        //定义HWGalil1 Ouput0 Nest type1 T3N
        static public IObit T3NestVacLeft             = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 1);
        static public IObit T3NestPufLeft             = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 2);
        static public IObit T3NestVacRight            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 3);
        static public IObit T3NestPufRight            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 4);
        static public IObit T3XferVac                 = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 5);
        static public IObit T3XferPuf                 = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 6);
        static public IObit T3Spare1Y07               = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 7);
        static public IObit T3Spare1Y08               = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 8);       
        static public IObit T3NestFinger              = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 9);

        //T1L,T1S,T1N T2N Output1-4
        static public IObit T1Nest12Finger              = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 1);
        static public IObit T1Nest34Finger              = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 2);
        static public IObit T1NestExtendZClamps         = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 3);        
        static public IObit T1SMAC12ExtendGripper       = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 4);
        static public IObit T1BridgeCameraLT1           = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 5);
        static public IObit T1BridgeCameraLT2           = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 6);
        static public IObit T1BridgeCamera1T3           = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 7);
        static public IObit T1BridgeCamera1T4           = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 8);
        //UM
        static public IObit UMEndeff1Vac            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 1);
        static public IObit UMEndeff1Puff           = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 2);
        static public IObit UMEndeff2Vac            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 3);
        static public IObit UMEndeff2Puff           = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 4);
        static public IObit UMEjctor12Vac           = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 5);
        static public IObit UMEjector12Puff         = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 6);
        static public IObit UMEjectorExtend12       = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 7);
        static public IObit UMEjectorRetract12      = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 8);

        //All modules
        static public IObit MchLJVReset             = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 10);
        static public IObit MchGearingTrigger       = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 11);
        static public IObit MchAbortOutput          = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 12);
        static public IObit MchLJV_PRG_1            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 13);
        static public IObit MchLJV_PRG_2            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 14);
        static public IObit MchLJV_PRG_3            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 15);
        static public IObit MchLJV_PRG_4            = new IObit(HWDevces.HWGalil1.IOOutput0Exp, 16);
        #endregion

        #region Galil2 IOs
        static public IObit BridgeLeftVacOK         = new IObit(HWDevces.HWGalil2.IOInput0, 1);
        static public IObit BridgeRightVacOK        = new IObit(HWDevces.HWGalil2.IOInput0, 2);
        static public IObit Galil2EstopRelayOK      = new IObit(HWDevces.HWGalil2.IOInput0, 3);
        static public IObit BridgeSMGripperClosed   = new IObit(HWDevces.HWGalil2.IOInput0, 1);

        static public IObit BridgeLeftVac           = new IObit(HWDevces.HWGalil2.IOInput0, 1);
        static public IObit BridgeLeftPuff          = new IObit(HWDevces.HWGalil2.IOInput0, 2);
        static public IObit BridgeRightVac          = new IObit(HWDevces.HWGalil2.IOInput0, 3);
        static public IObit BridgeRightPuff         = new IObit(HWDevces.HWGalil2.IOInput0, 4);

        static public IObit BridgeSMGripperExtend   = new IObit(HWDevces.HWGalil2.IOInput0, 1);
        static public IObit BridgeCamera1LT         = new IObit(HWDevces.HWGalil2.IOInput0, 2);
        static public IObit BridgeCamera2LT         = new IObit(HWDevces.HWGalil2.IOInput0, 3);
        static public IObit BridgeCamera1LT2        = new IObit(HWDevces.HWGalil2.IOInput0, 4);
        static public IObit BridgeCamera2LT2        = new IObit(HWDevces.HWGalil2.IOInput0, 5);


        #endregion

        #region GalilRIO IOs
        static public IObit UMNest1VacOK            = new IObit(HWDevces.HWGalilRIO.IOInput0, 1);
        static public IObit UMNest2VacOK            = new IObit(HWDevces.HWGalilRIO.IOInput0, 2);
        static public IObit UMNest3VacOK            = new IObit(HWDevces.HWGalilRIO.IOInput0, 3);
        static public IObit UMNest4VacOK            = new IObit(HWDevces.HWGalilRIO.IOInput0, 4);
        static public IObit UMEndeff1VacOK          = new IObit(HWDevces.HWGalilRIO.IOInput0, 5);
        static public IObit UMEndeff2VacOK          = new IObit(HWDevces.HWGalilRIO.IOInput0, 6);
        static public IObit UMEjector12VacOK        = new IObit(HWDevces.HWGalilRIO.IOInput0, 7);
        static public IObit UMEjector12Exgend       = new IObit(HWDevces.HWGalilRIO.IOInput0, 8);
        static public IObit UMEjector12Retract      = new IObit(HWDevces.HWGalilRIO.IOInput0, 9);
        static public IObit UMEndeff3VacOK          = new IObit(HWDevces.HWGalilRIO.IOInput0, 10);
        static public IObit UMEndeff4VacOK          = new IObit(HWDevces.HWGalilRIO.IOInput0, 11);
        static public IObit UMEjector34VacOK        = new IObit(HWDevces.HWGalilRIO.IOInput0, 12);
        static public IObit UMEjector34Exgend       = new IObit(HWDevces.HWGalilRIO.IOInput0, 13);
        static public IObit UMEjector34Retract      = new IObit(HWDevces.HWGalilRIO.IOInput0, 14);
        static public IObit UMNest1PartPresent      = new IObit(HWDevces.HWGalilRIO.IOInput0, 15);
        static public IObit UMNest2PartPresent      = new IObit(HWDevces.HWGalilRIO.IOInput0, 16);
        static public IObit UMNest3PartPresent      = new IObit(HWDevces.HWGalilRIO.IOInput0, 17);
        static public IObit UMNest4PartPresent      = new IObit(HWDevces.HWGalilRIO.IOInput0, 18);
        static public IObit UMRotatorVacOK          = new IObit(HWDevces.HWGalilRIO.IOInput0, 19);
        static public IObit UMBRC1OK                = new IObit(HWDevces.HWGalilRIO.IOInput0, 20);
        static public IObit UMBRC2OK                = new IObit(HWDevces.HWGalilRIO.IOInput0, 21);

        static public IObit UMNest1Vac              = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 1);
        static public IObit UMNest1Puff             = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 2);
        static public IObit UMNest12Finger          = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 3);
        static public IObit UMNest2Vac              = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 4);
        static public IObit UMNest2Puff             = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 5);
        static public IObit UMNest34Finger          = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 6);
        static public IObit UMNest3Vac              = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 7);
        static public IObit UMNest3Puff             = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 8);
        static public IObit UMNest4Vac              = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 9);
        static public IObit UMNest4Puff             = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 10);
        static public IObit UMEndeff3Vac            = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 11);
        static public IObit UMEndeff3Puff           = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 12);
        static public IObit UMEndeff4Vac            = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 13);
        static public IObit UMEndeff4Puff           = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 14);
        static public IObit UMEjector34Vac          = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 15);
        static public IObit UMEjector34Puff         = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 16);
        static public IObit UMExtendEjector34       = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 17);
        static public IObit UMRetractEjector34      = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 18);
        static public IObit UMRotatorVac            = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 19);
        static public IObit UMRotatorPuff           = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 20);
        static public IObit UMBCR1Trig              = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 21);
        static public IObit UMBCR2Trig              = new IObit(HWDevces.HWGalilRIO.IOOutput0Exp, 22);
        #endregion
    }
}
