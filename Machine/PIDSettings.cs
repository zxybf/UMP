using System;
using System.Globalization;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace DWFritz
{
   #region PID SETTINGS CLASS
   [Serializable, TypeConverter(typeof(PIDTypeConverter))] // So this class is expanded in property grid
   public struct PID
   {
      string id;  // Axis identifier (only used when displaying in property grid)
      bool  en;   // Axis Enable/disable flag
      float kp;   // Proportional Gain (KP * Current Position Error)
      float ki;   // Integral Gain (KI * Sum of Historical Position Error)
      float kd;   // Deravitive Gain (KD * delta Position (POS(t) - POS(t-1)), Helps in noise rejection
      float il;   // Integral Gain Limit (Limits maximum value of integral sum), Use negative sign on this parameter to disable KI during move
      float it;   // Smoothing Function (accel/decel, default = 1)
      long  st;   // Settling time in Milliseconds

      float sc;   // Scale (Counts/UnitOfMeasure)
      float sp;   // Speed (UnitOfMeassure/Sec)
      float spMax;// Maximum Speed
      float ac;   // Acceleration (UnitOfMeassure/Sec^2)
      float dc;   // Deceleration (UnitOfMeassure/Sec^2)
      float acMax;// Maximum Accel/decel
      int   ag;   // Amplifier Gain (1 = default gain, see docs)
      float tl;   // Torque Limit (volts) (0...9.99)
      float tk;   // Peak Torque Limit (volts) (0...9.99)
      float pl;   // Pole
      float fl;   // Forward Limit (UnitOfMeasure)
      float bl;   // Reverse Limit (UnitOfMeasure)
      float er;   // Maximum Following Error (UnitOfMeasure)
      float ho;   // Home offset (UnitOfMeasure)
      float fa;   // Feedforward Accel
      float fv;   // Feedforward Velocity
      
      public PID(string _id)
      {
         id = _id;
         en = true;
         kp = 6f;
         ki = 0.1f;
         kd = 40f;
         il = 1f;
         it = 1f;
         st = 20;

         sc = 10000;
         sp = 50;
         spMax = 10;
         ac = 100;
         dc = 100;
         acMax = 100;
         ag = 1;
         tl = 3;
         tk = 5;
         pl = 0;
         fl = 500;
         bl = -500;
         er = 5f;
         ho = 0f;
         fa = 0f;
         fv = 0f;
      }
      
      /// <summary>
      /// Creates new PID with _id, and settings from _other
      /// </summary>
      /// <param name="_id"></param>
      /// <param name="_other"></param>
      public PID(string _id, PID _other)
      {
         id    = _id;
         en    = _other.en;
         kp    = _other.kp;
         ki    = _other.ki;
         kd    = _other.kd;
         il    = _other.il;
         it    = _other.it;
         st    = _other.st;
         sc    = _other.sc;
         sp    = _other.sp;
         spMax = _other.spMax;
         ac    = _other.ac;
         dc    = _other.dc;
         acMax = _other.acMax;
         ag    = _other.ag;
         tl    = _other.tl;
         tk    = _other.tk;
         pl    = _other.pl;
         fl    = _other.fl;
         bl    = _other.bl;
         er    = _other.er;
         ho    = _other.ho;
         fa    = _other.fa;
         fv    = _other.fv;
      }

      /// <summary>
      /// Constructor to initialize the PID Settings for a single axis
      /// </summary>
      /// <param name="_id">Axis Identifier (human readable name, not used for anything)</param>
      /// <param name="_en">Axis Enable/Disable</param>
      /// <param name="_kp">Proportional Gain</param>
      /// <param name="_ki">Integral Gain</param>
      /// <param name="_kd">Derivative Gain</param>
      /// <param name="_il">Integrator Limit (negative only integrats at end of profile)</param>
      /// <param name="_it">Smoothing Funtcion (accel/decel) default = 1</param>
      /// <param name="_st">Settleing Time (milliseconds)</param>
      /// <param name="_sc">Scale (counts/measurement_unit)</param>
      /// <param name="_sp">Speed (UnitOfMeasure/sec)</param>
      /// <param name="_spMax">Max Speed (UnitOfMeasure/sec)</param>
      /// <param name="_ac">Acceleration (UnitOfMeasure/sec^2)</param>
      /// <param name="_dc">Deceleration (UnitOfMeasure/sec^2)</param>
      /// <param name="_acMax">Maximum allowed acceleration</param>
      /// <param name="_ag">Amplifier Gain (default=1)</param>
      /// <param name="_tl">Torque Limit (volts 0...9.99)</param>
      /// <param name="_tk">Peak Torque Limit (volts 0...9.99)</param>
      /// <param name="_pl">Pole Filter</param>
      /// <param name="_fl">Forward Limit (UnitOfMeasure)</param>
      /// <param name="_bl">Reverse Limit (UnitOfMeasure)</param>
      /// <param name="_er">Maximum Following Error (UnitOfMeasure)</param>
      /// <param name="_ho">Home Offset (UnitOfMeasure)</param>
      /// <param name="_fa">Feedforward Acceleration (See Doc for units)</param>
      /// <param name="_fv">Feedforward Velocity (See Doc for units)</param>
      public PID(string _id, bool _en, float _kp, float _ki, float _kd, float _il, float _it, long _st,
                 float  _sc, float _sp, float _spMax, float _ac, float _dc, float _acMax,
                 int    _ag, float _tl, float _tk, float  _pl, float _fl, float _bl, float _er, float _ho,
                 float  _fa, float _fv)
      {
         id    = _id;
         en    = _en;
         kp    = _kp;
         ki    = _ki;
         kd    = _kd;
         il    = _il;
         it    = _it;
         st    = _st;

         sc    = _sc;
         sp    = _sp;
         spMax = _spMax;
         ac    = _ac;
         dc    = _dc;
         acMax = _acMax;
         ag    = _ag;
         tl    = _tl;
         tk    = _tk;
         pl    = _pl;
         fl    = _fl;
         bl    = _bl;
         er    = _er;
         ho    = _ho;
         fa    = _fa;
         fv    = _fv;
      }

      public PID(PID _other)
      {
         id    = "";
         en    = _other.en;
         kp    = _other.kp;
         ki    = _other.ki;
         kd    = _other.kd;
         il    = _other.il;
         it    = _other.it;
         st    = _other.st;

         sc    = _other.sc;
         sp    = _other.sp;
         spMax = _other.spMax;
         ac    = _other.ac;
         dc    = _other.dc;
         acMax = _other.acMax;
         ag    = _other.ag;
         tl    = _other.tl;
         tk    = _other.tk;
         pl    = _other.pl;
         fl    = _other.fl;
         bl    = _other.bl;
         er    = _other.er;
         ho    = _other.ho;
         fa    = _other.fa;
         fv    = _other.fv;
      }

      
      #region OVERRIDES
      public override string ToString()
      {
         return   ID;
      }
      

      public override bool Equals(object obj)
      {
         if (obj == null || GetType() != obj.GetType()) return false;
         PID _src= (PID)obj;
         if ((id == _src.id) && (en == _src.en) && 
             (kp == _src.kp) && (ki == _src.ki) && (kd == _src.kd) && (il == _src.il) && (it == _src.it) && (st == _src.st) && 
             (sc == _src.sc) && (sp == _src.sp) && (spMax == _src.spMax) &&
             (ac == _src.ac) && (dc == _src.dc) && (acMax == _src.acMax) &&
             (ag == _src.ag) && (tl == _src.tl) && (tk == _src.tk) && 
             (pl == _src.pl) && (fl == _src.fl) && (bl == _src.bl) && (er == _src.er) && (ho == _src.ho) &&
             (fa == _src.fa) && (fv == _src.fv))
         {
            return true;
         }
         return false;
      }

      public override int GetHashCode()
      {
         //return base.GetHashCode();
         return (int)((int)(kp*1000) ^ (int)(ki*1000) ^ (int)(kd*1000) ^ (int)(il*1000) ^ (int)(st*1000));
      }
      #endregion OVERRIDES

      #region ACCESSORS
      /// <summary>
      /// Axis identifier (only used when displaying in property grid)
      /// </summary>
      [Description("Axis Name"), ReadOnly(true)]
      public string ID { get { return id; } set { id = value; } }
      
      /// Axis Enable switch (used when system does not have the specified axis)
      [Description("Axis Enable/Disable")]
      public bool EN { get { return en; } set { en = value; } }

      /// <summary>
      /// Proportional Gain (KP * Current Position Error)
      /// </summary>
      [Description("Proportional Gain (KP * Current Position Error)")]
      public float KP { get { return kp; } set { kp = value; } }
      
      /// <summary>
      /// Integral Gain (KI * Sum of Historical Position Error)
      /// </summary>
      [Description("Integral Gain (KI * Sum of Historical Position Error)")]
      public float KI { get { return ki; } set { ki = value; } }

      /// <summary>
      /// Deravitive Gain (KD * delta Position (POS(t) - POS(t-1))
      /// Helps in noise rejection
      /// </summary>
      [Description("Deravitive Gain (KD * delta Position (POS(t) - POS(t-1))\r\n" +
                            "Helps in noise rejection")]
      public float KD { get { return kd; } set { kd = value; } }

      /// <summary>
      /// Integral Gain Limit (Limits maximum value of integral sum)
      /// Use negative sign on this parameter to disable KI during move
      /// </summary>
      [Description("Integral Gain Limit (Limits maximum value of integral sum)\r\n" +
                           "Use negative sign on this parameter to disable KI during move")]
      public float IL { get { return il; } set { il = value; } }

      /// <summary>
      /// Smoothing Function (accel/decel), (default = 1)
      /// </summary>
      [Description("Smoothing Function (accel/decel), (default = 1)")]
      public float IT { get { return it; } set { it = value; } }

      /// <summary>Settling time in Milliseconds</summary>
      [Description("Settling time in Milliseconds")]
      public long ST { get { return st; } set { st = value; } }

      /// <summary>Scale (Counts/UnitOfMeasure)</summary>
      [Description("Scale (Counts/UnitOfMeasure)")]
      public float SC { get { return sc; } set { sc = value; } }
      
      /// <summary>Speed (UnitOfMeassure/Sec)</summary>
      [Description("Speed (UnitOfMeasure/Sec)")]
      public float SP { get { return sp; } set { sp = value; } }

      /// <summary>Speed Max (UnitOfMeassure/Sec)</summary>
      [Description("READ-ONLY!!  Speed (UnitOfMeasure/Sec)"), ReadOnly(true)]
      public float SP_MAX { get {return spMax;} set{spMax=value;}}

      /// <summary>Acceleration (UnitOfMeassure/Sec^2)</summary>
      [Description("Acceleration (UnitOfMeasure/(Sec^2))")]
      public float AC { get { return ac; } set { ac = value; } }

      /// <summary>Deceleration (UnitOfMeassure/Sec^2)</summary>
      [Description("Deceleration (UnitOfMeasure/(Sec^2))")]
      public float DC { get { return dc; } set { dc = value; } }

      /// <summary>Accel/Decel Max (UnitOfMeassure/Sec^2)</summary>
      [Description("READ-ONLY!!  Max Accel/Decel (UnitOfMeasure/Sec^2)"), ReadOnly(true)]
      public float AC_MAX { get {return acMax;} set{acMax=value;}}

      /// <summary>Amplifier Gain (default = 1)</summary>
      [Description("READ-ONLY!!  Amplifier Gain (default = 1)"), ReadOnly(false)]  // Do not allow any changes!!
      public int AG { get { return ag; } set { ag = value; } }
      
      /// <summary>Torque Limit (volts) (0...9.99)</summary>
      [Description("Torque Limit (volts) (0...9.99)")]
      public float TL { get { return tl; } set { tl = value; } }
      
      /// <summary>Peak Torque Limit (volts) (0...9.99)</summary>
      [Description("Peak Torque Limit (volts) (0...9.99)")]
      public float TK { get { return tk; } set { tk = value; } }
      
      /// <summary>Pole (0 .. 0.999)
      /// PL Calculations
      /// TM = 1000
      /// T = TM/1,000,000
      /// F = (Crossover Freq HZ) = 1000   750   500   383   190   170   150   130   110    90    70
      /// PL = e(-T*F*2*PI)       = 0.04  0.10  0.20  0.30  0.55  0.59  0.62  0.66  0.71  0.75  0.80
      /// </summary>
      [Description("Pole (0 .. 0.999)")]
      public float PL { get { return pl; } set { pl = value; } }

      /// <summary>Forward Limit (UnitOfMeasure)</summary>
      [Description("Forward Limit (UnitOfMeasure)")]
      public float FL { get { return fl; } set { fl = value; } }

      /// <summary>Reverse Limit (UnitOfMeasure)</summary>
      [Description("Reverse Limit (UnitOfMeasure)")]
      public float BL { get { return bl; } set { bl = value; } }

      /// <summary>Maximum Following Error (UnitOfMeasure)</summary>
      [Description("Maximum Following Error (UnitOfMeasure)")]
      public float ER { get { return er; } set { er = value; } }

      /// <summary>Maximum Following Error (UnitOfMeasure)</summary>
      [Description("Home Offset (UnitOfMeasure) from found encoder index position\r\n" +
                            "After all axes have been homed, axis is moved by this amount and the Zero point is defined at the new position")]
      public float HO { get { return ho; } set { ho = value; } }

      /// <summary>FF Acceleration</summary>
      [Description("FF Acceleration")]
      public float FA { get { return fa; } set { fa = value; } }

      /// <summary>FF Velocity</summary>
      [Description("FF Velocity")]
      public float FV { get { return fv; } set { fv = value; } }

      #endregion ACCESSORS

      #region HELPERS
      /// <summary>AC in counts per second^2 (AC * SC)</summary>
      public long AC_Counts() { return (long )(ac * sc); }

      /// <summary>DC in counts per second^2 (DC * SC)</summary>
      public long DC_Counts() { return (long)(dc * sc); }

      /// <summary>SP in counts per second (SP * SC)</summary>
      public long SP_Counts() { return (long)(sp * sc); }

      /// <summary>FL in counts (FL * SC)</summary>
      public long FL_Counts() { return (long)(fl * sc); }

      /// <summary>BL in counts (BL * SC)</summary>
      public long BL_Counts() { return (long)(bl * sc); }

      /// <summary>ER in counts (ER * SC)</summary>
      public long ER_Counts() { return (long)(er * sc); }

      /// <summary>HO in counts (HO * SC)</summary>
      public long HO_Counts() { return (long)(ho * sc); }
      #endregion HELPERS

      public string Validate(PID _defaults)
      {
         if (en == false)
         {
            return null;
         }
         float _spMax = _defaults.spMax;
         float _acMax = _defaults.acMax;
         if (spMax > _defaults.spMax) spMax = _defaults.spMax;
         if (acMax > _defaults.acMax) acMax = _defaults.acMax;
/*         if (ag != _defaults.ag)
         {
            ag = _defaults.ag; 
         } */
         string errs = "";
         if (kp < 0)                   errs += id + ".KP must be >= 0\r\n";
         if (ki < 0 || ki > 255)       errs += id + ".KI range [0 ... 255]\r\n";
         if (kd < 0)                   errs += id + ".KD must be >= 0\r\n";
         if (il < -10 || il > 10)      errs += id + ".IL range [-10 ... 10]\r\n";
         if (it < 0.004f || it > 1)    errs += id + ".IT range [0.004 ... 1.0]\r\n";
         if (st < 0 || st > 2000)      errs += id + ".ST range [0 ... 2000]\r\n";

         if (sc <= 0)                  errs += id + ".SC must be > 0\r\n";
         if (spMax <= 0)               errs += id + ".SP_MAX range [0+ ... PID_DEFAULT.SP_MAX]\r\n";
         if (acMax <= 0)               errs += id + ".AC_MAX range [0+ ... PID_DEFAULT.AC_MAX]\r\n";
         if (sp <= 1 || sp > spMax)    errs += id + ".SP range [1+ ... " + spMax.ToString() + "]\r\n";
         if (ac <= 1 || ac > acMax)    errs += id + ".AC range [1+ ... " + acMax.ToString() + "]\r\n";
         if (dc <= 1 || dc > acMax)    errs += id + ".DC range [1+ ... " + acMax.ToString() + "]\r\n";
         if (tl <= 0 || tl > 9.998)    errs += id + ".TL range [0+ ... 9.998]\r\n";
         if (tk <= 0 || tk > 9.998)    errs += id + ".TK range [0+ ... 9.998]\r\n";
         if (pl < 0 || pl > 0.9999f)   errs += id + ".PL range [0+ ... 0.9999]\r\n";
         if ((fl < bl)|| (bl > fl))    errs += id + ".FL mus be > .bl\r\n";
         if (er < 0)                   errs += id + ".ER must be >= 0\r\n";
       //if (ho < ??? || ho > ???)     errs += id + ".HO ???\r\n";
         if (fa < 0)                   errs += id + ".FA must be >= 0\r\n";
         if (fv < 0)                   errs += id + ".FV must be >= 0\r\n";

         if (errs != "")
         {
            return errs;
         }
         return null;
      }
   }
   #endregion PID SETTINGS STRUCTURE

   #region PIDTypeConverter Class
   /// <summary>
   /// Class used as a converter for the Doofer class .. 
   /// Example Source:  http://www.morganskinner.com/Articles/AStructTypeConverter/
   /// </summary>
   public class PIDTypeConverter : TypeConverter
   {
      /// <summary>
      /// Can the framework call CreateInstance?
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      public override bool GetCreateInstanceSupported ( ITypeDescriptorContext context )
      {
          return true;// Yes!
      }

      /// <summary>
      /// Satisfy the CreateInstance call by reading data from the propertyValues dictionary
      /// </summary>
      /// <param name="context"></param>
      /// <param name="propertyValues"></param>
      /// <returns></returns>
      public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
      {
         // Get values of properties from the dictionary, and
         // create a new instance which is returned to the caller
         string pA = (string)propertyValues["ID"];
         bool   pB =   (bool)propertyValues["EN"];
         float  pC =  (float)propertyValues["KP"];
         float  pD =  (float)propertyValues["KI"];
         float  pE =  (float)propertyValues["KD"];
         float  pF =  (float)propertyValues["IL"];
         float  pG =  (float)propertyValues["IT"];
         long   pH =   (long)propertyValues["ST"];
         float  pI =  (float)propertyValues["SC"];
         float  pJ =  (float)propertyValues["SP"];
         float  pK =  (float)propertyValues["SP_MAX"];
         float  pL =  (float)propertyValues["AC"];
         float  pM =  (float)propertyValues["DC"];
         float  pN =  (float)propertyValues["AC_MAX"];
         int    pO =    (int)propertyValues["AG"];
         float  pP =  (float)propertyValues["TL"];
         float  pQ =  (float)propertyValues["TK"];
         float  pR =  (float)propertyValues["PL"];
         float  pS =  (float)propertyValues["FL"];
         float  pT =  (float)propertyValues["BL"];
         float  pU =  (float)propertyValues["ER"];
         float  pV =  (float)propertyValues["HO"];
         float  pW =  (float)propertyValues["FA"];
         float  pX =  (float)propertyValues["FV"];

         return new PID(pA,pB,pC,pD,pE,pF,pG,pH,pI,pJ,pK,pL,pM,pN,pO,pP,pQ,pR,pS,pT,pU,pV,pW,pX);
      }
       
      /// <summary>
      /// Does this struct expose properties?
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      public override bool GetPropertiesSupported (ITypeDescriptorContext context)
      {
         return true;   // Yes!
      }

      /// <summary>
      /// Return the properties of this struct
      /// </summary>
      /// <param name="context"></param>
      /// <param name="value"></param>
      /// <param name="attributes"></param>
      /// <returns></returns>
      public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
      {
         PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value, attributes);
         return properties;
      }

      public override bool  CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      {
         return false;  // Cannot convert from anything, only allowing editing of individual properties
         //return base.CanConvertFrom(context, sourceType);
         // Just strings for now
         //bool canConvert = (sourceType == typeof(string));

         //if (!canConvert)
         //{
         //   canConvert = base.CanConvertFrom(context, sourceType);
         //}
         //return canConvert;
      }
      
      public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
      {
         if ((string)value != null)
         {
            return new PID((string)value);
         }
         return base.ConvertFrom(context, culture, value);
      }
      
      public override bool  CanConvertTo(ITypeDescriptorContext context, Type destinationType)
      {
         // InstanceDescriptor is used in the code behind
         bool canConvert = (destinationType == typeof(InstanceDescriptor));
         if (!canConvert)
         {
            canConvert = base.CanConvertFrom(context, destinationType);
         }
         return canConvert;
      }
      
      public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
                                       object value, Type destinationType)
      {
         object retVal = null;
         PID p = (PID)value;

         // If this is an instance descriptor...
         if (destinationType == typeof(InstanceDescriptor))
         {
            Type[] argTypes = new System.Type[] {
                                                  typeof(string),
                                                  typeof(bool),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(long),

                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(int),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float),
                                                  typeof(float)
                                                 };

            // Lookup the appropriate constructor
            ConstructorInfo constructor = typeof(PID).GetConstructor(argTypes);

            object[] arguments = new object[] { p.ID, p.EN,
                                                p.KP, p.KI,       p.KD, p.IL, p.IT,    p.ST, p.SC,
                                                p.SP, p.SP_MAX,   p.AC, p.DC, p.AC_MAX,
                                                p.AG, p.TL,       p.TK, p.PL, p.FL,    p.BL,
                                                p.ER, p.HO,       p.FA, p.FV};

            // And return an instance descriptor to the caller. Will fill in the CodeBehind stuff in VS.Net
            retVal = new InstanceDescriptor(constructor, arguments);
         }
         else if (destinationType == typeof(string))
         {
            //retVal = p.ToString();
            retVal = null; // prevents ID from showing at top level of struct in prop grid
         }
         else
         {
            retVal = base.ConvertTo(context, culture, value, destinationType);
         }

         return retVal;
      }
   }
   #endregion PIDTypeConverter Class
}
