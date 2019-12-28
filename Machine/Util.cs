//*****************************************************************************
//*         FILE: Util.cs                     C. Voigt
//*  DESCRIPTION: Utility Classes
//*               
//*               Class Stats   - StdDeviation, min, max, ave calculations on
//*                               accumulated data (also setup for viewing
//*                               in a propertygrid control).
//*
//*               Class FPoint  - An XY coordinate using Floating point numbers
//*
//*               Class Util    - A collection of Static Utility methods for use
//*                               throughout your application
//*                     - ReadableEnum (converts _ and hungarian notation
//*                                               to human readable form).
//*                     - Interpolate - Mathematical interpolation
//*                     - Assert  - Assertions from your code which displays the UserPrompt and call stack
//*                     - CallStack - returns the call stack as a string
//*****************************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
//using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Reflection;
using System.Security.Principal;
using System.Runtime.CompilerServices;


namespace Machine
{   
   public delegate void StatusHandler(string log);  // Method signature for status logging.

   public static class Util
   {
      public static bool NO_SQL { get; set; }

      private static string sTcpAckFrequency  = "TcpAckFrequency";
      private static string sTCPNoDelay       = "TCPNoDelay"; 
      private static string sEnableDHCP       = "EnableDHCP";
      private static string keyPath           = "SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\";

      public static event StatusHandler OnStatus;

        public class IntBits : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private int bits;
            public IntBits(int intValue) 
            { 
                bits = intValue; 
                NotifyPropertyChanged();
            }
            /// <summary>
            /// 用其他intBit新建一个独立生成的新IntBit
            /// </summary>
            /// <param name="otherBits"></param>
            public IntBits(IntBits otherBits) => bits = otherBits.BitsToInt;

            /// <summary>
            /// 改变int的index位，通知NotifyPropertyChanged("Bit"+index.ToString());
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public bool this[int index]
            {
                get => (bits & (1 << index)) != 0;
                set
                {
                    if (value)
                    {
                        bits |= (1 << index);
                    }
                    else
                        bits &= ~(1 << index);
                    if (index > 8)
                        NotifyPropertyChanged("Bit" + (index + 1).ToString());
                    else
                        NotifyPropertyChanged("Bit0" + (index + 1).ToString());
                }
            }

            /// <summary>
            /// 返回int 类型的Bit
            /// </summary>
            public int GetBits() => bits;
            /// <summary>
            /// 二进制表示Bit
            /// </summary>
            public string BitsToString => Convert.ToString(bits, 2);

            public int BitsToInt 
            { 
                get => bits; 
                set 
                {
                    if (bits != value)
                    {
                        int rsl = bits ^ value;
                        Util.IntBits intBits = new Util.IntBits(rsl);
                        for (int i = 0; i < 16; i++)
                        {
                            if (intBits[i])
                            {
                                this[i] = !this[i];
                            }
                        }
                        //bits = value; NotifyPropertyChanged();
                    }
                } 
            }
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public static string CleanupFileName ( string fname )
      {
        if ( String.IsNullOrEmpty( fname ) )
        {
          return "No-File-Name";
        }
        string fName = "";
        char[] invalidChars = Path.GetInvalidFileNameChars( );
        fName = fname.Trim( );
        char[] nameArray = fName.ToCharArray( );
        foreach ( char c in invalidChars )
        {
          if ( Array.IndexOf( nameArray, c ) > -1 )
          {
            fName = fName.Replace( c, '_' );
          }
        }
        return fName;
      }

      public static string CleanupFolderName ( string fname )
      {
        if ( String.IsNullOrEmpty( fname ) )
        {
          return "No-Path-Name";
        }
        string fName = "";
        char[] invalidChars = Path.GetInvalidPathChars( );
        fName = fname.Trim( );
        char[] nameArray = fName.ToCharArray( );
        foreach ( char c in invalidChars )
        {
          if ( Array.IndexOf( nameArray, c ) > -1 )
          {
            fName.Replace( c, '_' );
          }
        }
        return fName;
      }

      /// <summary>
      /// Allow only chars '0' through '9' and one decimal point '.'
      /// </summary>
      /// <param name="str"></param>
      /// <returns></returns>
      public static string CleanupNumberString(string str)
      {
         if (str == null)
         {
            return "0";
         }
         if (str == "")
         {
            return "0";
         }
         str = str.Trim();

         string num = "";
         bool bDecimalPointFound = false;
         for (int i = 0; i < str.Length; i++)
         {
            char c = str[i];
            if (('0' <= c) && (c <= '9'))
            {
               num += c;
            }
            else if (c == '.' && !bDecimalPointFound)
            {
               bDecimalPointFound = true;
               num += c;
            }
            else if ((c == '-') && (i == 0))
            {
               num += c;
            }
         }
         return num;
      }

      public static string RemoveUnderscores(string s)
      {
         if (s == null)
         {
            return s;
         }
         return s.Replace('_', ' ');
      }

      public static string AddSpacesBeforeCaps(string s)
      {
         if (s == null)
         {
            return s;
         }
         string ret = s;

         
         // Insert a space before every caps letter (except the first one.)\
         char C;
         for (C = 'A'; C <= 'Z'; C++)
         {
            string S = new string(C, 1);
            string spaceS = " " + C;
            ret = ret.Replace(S, spaceS);
         }
         ret = ret.Replace("  ", " "); // Replace any double spaces with a single space.
         return ret;
      }
      /// <summary>
      /// Converts Underscores to spaces, and puts spaces before each capitol letter except for first letter.
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public static string ReadableEnum(string s)
      {
         if (s != null)
         {
            return AddSpacesBeforeCaps(RemoveUnderscores(s)).Trim();
         }
         else
         {
            return s;
         }
      }

      
      // Given known value x, and points x,x0,y,y0, return data point y between y0 and y1
      public static double Interpolate(double x, double x0, double x1, double y0, double y1)
      {
         return y0 + (x - x0) * ((y1 - y0) / (x1 - x0));
      }


      /// <summary>
      /// If condition is false, an assertion is thrown, and call stack logged to logfile.
      /// Returns flag.
      /// </summary>
      /// <param name="flag"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public static bool Assert(bool flag, string message)
      {
         try
         {
            if (!flag)
            {
               
               System.Diagnostics.Debug.Assert(flag, message); // Only displayed for debug builds.
               
               // Log the Callstack, but don't log the first 4 items.. they are related to this function and 
               // the TraceEventCache function implementation...
               
               System.Diagnostics.TraceEventCache tec = new System.Diagnostics.TraceEventCache();
               
               
               string cs = tec.Callstack;
               string []splitSep = {"\r\n"};
               string []stack = cs.Split(splitSep, StringSplitOptions.RemoveEmptyEntries);
               cs = "";
               for (int i = 4; i < stack.Length; i++)
               {
                  cs += "\t" + stack[i] + "\r\n";
               }
               if (OnStatus != null)
               {
                  OnStatus("SW ASSERT:  " + message + "\r\n" + cs);
               }
            }
         }
         catch
         {
         }
         return flag;   // Return status of test so we can use DWF_Assert in conditional statements
      }

      public static string CallStack()
      {
         // Return the Callstack.
         System.Diagnostics.TraceEventCache tec = new System.Diagnostics.TraceEventCache();
         return tec.Callstack;
      }
      
      /// <summary>
      /// Serializes the passed in object into a data byte stream for transmission
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public static byte[] Serialize(object obj)
      {
         try
         {
            IFormatter formatter = new BinaryFormatter();
   //         formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
            MemoryStream memStr = new MemoryStream();
            formatter.Serialize(memStr, obj);
            byte [] buffer = memStr.GetBuffer();
            return buffer;
         }
         catch (Exception ex)
         {
            //System.Windows.Forms.MessageBox.Show("Add [Serializable] Attribute before class being serialized\r\n" + ex.Message);
            System.Diagnostics.Debug.Fail("Add [Serializable] Attribute before class being serialized\r\n" + ex.Message);
            }
         return null;
      }
      
      public static Object Deserialize(byte [] data)
      {
         try
         {
            IFormatter formatter = new BinaryFormatter();
   //         formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
            return formatter.Deserialize(new MemoryStream(data));
         }
         catch
         {
            return new object();
            // Sometimes the data may be corrupt and cannot be deserialized...
            // There is really nothing we can do except check
            // for a null pointer in the calling code and handle it.
         }
      }
      
      public static string TIMESTAMP()
      {
         return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      }
      
      /// <summary>
      /// Timestamp formated as yyyy/mm/dd hh:mm:ss
      /// </summary>
      /// <param name="dt"></param>
      /// <returns></returns>
      public static string TIMESTAMP(DateTime dt)
      {
         return dt.ToString("yyyy/MM/dd HH:mm:ss");
      }
      /// <summary>
      /// Formats passed in DateTime for yyyyMMdd_HHmmss
      /// </summary>
      /// <param name="fmt"></param>
      /// <returns></returns>
      public static string FILE_TIMESTAMP(DateTime dt)
      {
         return dt.ToString("yyyyMMdd_HHmmss");
      }
      
      /// <summary>
      /// Returns current date time string for filenames
      /// </summary>
      /// <returns></returns>
      public static string FILE_TIMESTAMP()
      {
         DateTime dt = DateTime.Now;
         return FILE_TIMESTAMP(dt);
      }
      
      /// <summary>
      /// format string must have {0} in it for placement of the timestamp
      /// </summary>
      /// <param name="fmt"></param>
      /// <returns></returns>
      public static string INSERT_TIMESTAMP(string fmt)
      {
         return string.Format(fmt, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
      }
      
      public static void MakeFileWriteable(string path)
      {
         FileInfo fi = new FileInfo(path);
         if (fi.Exists)
         {
            if (fi.IsReadOnly)
            {
               fi.Attributes = fi.Attributes ^ FileAttributes.ReadOnly; // Clear the Readonly flag.
            }
         }
      }

      public static void EndThisProcess()
      {
         System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
         p.Kill();
      }

      /// <summary>
      /// Attempts to ping the vision system to make sure it is there and connected.
      /// </summary>
      /// <returns>empty string ("") if successfull, otherwise error message</returns>
      public static string Ping(string ip, string desc)
      {
         for (int i = 0; i < 5; i++)
         {
            Ping pingSender = new Ping ();
            PingOptions options = new PingOptions ();

            // but change the fragmentation behavior.
            options.Ttl = 3;  // Maximum number of hops to find client.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "HELLO";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 500;
            PingReply reply = pingSender.Send(ip, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
               System.Diagnostics.Trace.WriteLine(desc + ":  Address: " +          reply.Address.ToString ());
               System.Diagnostics.Trace.WriteLine(desc + ":  RoundTrip time: " +   reply.RoundtripTime);
               System.Diagnostics.Trace.WriteLine(desc + ":  Time to live: " +     reply.Options.Ttl);
               System.Diagnostics.Trace.WriteLine(desc + ":  Don't fragment: " +   reply.Options.DontFragment);
               System.Diagnostics.Trace.WriteLine(desc + ":  Buffer size: " +      reply.Buffer.Length);
               return "";  // Success
            }
            System.Threading.Thread.Sleep(1000);
         }
         return desc + ": Connection Error, Ping Failed on " + ip;
      }
      
        /*
      /// <summary>
      /// Sets TcpAckFrequency  and TCPNoDelay to 1 for all Non-DHCP nics in the system
      /// The TcpAckFrequency value specifies the number of ACKs that will be outstanding before the delayed ACK timer is ignored.
      /// </summary>
      public static void SetTcpRegSettings()
      {
       
          if (CheckTcpReg() == 0)    {  return; }
          if(!IsWindowsAdmin_ADMIN())
          {
            UserPrompt.ShowPrompt("Registry update required.\n\r1) Close the GUI and log into Windows as Admin User\n\r2) Run the GUI as Administrator" + 
            "(right click GUI.exe => Run as Administrator)", "UPDATE REQUIRED");
            return;
          }

          int     _count            = 0;
          try
          {
              RegistryKey interfaceKey = Registry.LocalMachine;
              interfaceKey = interfaceKey.OpenSubKey(keyPath, true);
              foreach (string keyName in interfaceKey.GetSubKeyNames())
              {
                  RegistryKey iKey = Registry.LocalMachine;
                  iKey = iKey.OpenSubKey(keyPath + keyName, true);

                  int _iDhcpVal = 0;
                  try 
                  { 
                     _iDhcpVal = (int)iKey.GetValue(sEnableDHCP); 
                  }
                  catch 
                  { 
                     continue; 
                  }
                  if (_iDhcpVal == 0)
                  {
                      int _iAckValCurr    = (int)iKey.GetValue(sTcpAckFrequency, -1);    // if key not found returns -1
                      int _iNoDelyalCurr  = (int)iKey.GetValue(sTCPNoDelay, -1);       // if key not found returns -1
                      if (_iAckValCurr != 1 || _iNoDelyalCurr != 1)
                      {
                          iKey.SetValue(sTcpAckFrequency, 1);
                          iKey.SetValue(sTCPNoDelay, 1);
                          _count++;
                      }
                  }
              }
          }
          catch (Exception ex)
          {
            System.Windows.Forms.MessageBox.Show( ex.ToString(), "Registry Update");
          }

          if(_count != 0)
          { 
              UserPrompt.ShowPrompt( "Registry updated - Reboot required", "UPDATE, count: " +_count.ToString());
          }
      }
        */
      /// <summary>
      /// Returns the number of Registry entries that need to be updated.
      /// </summary>
      /// <returns></returns>
      public static int CheckTcpReg()
      {
          int     _count            = 0;

            try
            {
                RegistryKey interfaceKey = Registry.LocalMachine;
                interfaceKey = interfaceKey.OpenSubKey(keyPath, false);
                foreach (string keyName in interfaceKey.GetSubKeyNames())
                {
                    RegistryKey iKey = Registry.LocalMachine;
                    iKey = iKey.OpenSubKey(keyPath + keyName, false);

                    int _iDhcpVal = 0;
                    try
                    {
                        _iDhcpVal = (int)iKey.GetValue(sEnableDHCP);
                    }
                    catch
                    {
                        continue;
                    }
                    if (_iDhcpVal == 0)
                    {
                        if ((int)iKey.GetValue(sTcpAckFrequency, -1) != 1)
                        {
                            _count++;
                        }
                        if ((int)iKey.GetValue(sTCPNoDelay, -1) != 1)
                        {
                            _count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show( ex.ToString(), "Registry check");
                System.Diagnostics.Debug.Fail(ex.ToString());
            }
          return _count;
      }

      // http://stackoverflow.com/questions/11660184/c-sharp-check-if-run-as-administrator
      // Check that the application was started with elevated administrator rights (right click, run an Administrator)
      private static bool IsWindowsAdmin_ADMIN()
      {
         WindowsIdentity identity = WindowsIdentity.GetCurrent();
         WindowsPrincipal principal = new WindowsPrincipal(identity);
         return principal.IsInRole(WindowsBuiltInRole.Administrator);
      }


      /// <summary>
      /// Removes leading zeros in each octet
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      public static string CleanUpIpAddress(string _ipAddress)
      {
         string[] s = _ipAddress.Split(new char[]{'.'});
         if (s.Length == 4)
         {
            int i1 = 0;
            int i2 = 0;
            int i3 = 0;
            int i4 = 0;
            if (int.TryParse(s[0], out i1) && int.TryParse(s[1], out i2) &&
                int.TryParse(s[2], out i3) && int.TryParse(s[3], out i4))
            {
               return i1.ToString() + "." + i2.ToString() + "." + i3.ToString() + "." + i4.ToString();
            }
         }
            return _ipAddress;
      }

      public static bool FileSafeWrite(string filepath, object obj, string encyptionKey = null)
      {
          byte[] fileData = Serialize(obj);
          if (!string.IsNullOrEmpty(encyptionKey))
          {
              fileData = Encrypt(fileData, encyptionKey);
          }
          // Delete Backup Files
          File.Delete(filepath + ".old");
          File.Delete(filepath + ".tmp");
          // Write data to a temp file
          File.WriteAllBytes(filepath + ".tmp", fileData);
          // if succesful, rename current file to .bak
          if (File.Exists(filepath))
          {
              File.Move(filepath, filepath + ".old");
          }
          // Rename temp to final
          File.Move(filepath + ".tmp", filepath);
          // Delete Backup file, ignore errors
          File.Delete(filepath + ".old");
          File.Delete(filepath + ".tmp");

         return true;
      }

      public static object FileSafeRead(string filepath, string encryptionKey = null)
      {
          byte[] fileData = null;
          if (File.Exists(filepath))
          {
              fileData = File.ReadAllBytes(filepath);
          }
          else if (File.Exists(filepath + ".tmp"))
          {
              fileData = File.ReadAllBytes(filepath + ".tmp");
          }
          
          if (!string.IsNullOrEmpty(encryptionKey) && fileData != null)
          {
              fileData = Decrypt(fileData, encryptionKey);
          }
          if (fileData != null)
          {
              return Deserialize(fileData);
          }
          return null;
      }

      #region ENCRYPTION BYTES
      internal static byte[] Encrypt(byte[] plainData, string sKey = null)
      {
          if (sKey == null)
          {
              sKey = "";
          }
          DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
          DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
          DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
          ICryptoTransform desEncrypt = DES.CreateEncryptor();
          byte[] encryptedData = desEncrypt.TransformFinalBlock(plainData, 0, plainData.Length);

          return encryptedData;
      }

      public static byte[] Decrypt(byte[] encryptedData, string sKey = null)
      {
          if (sKey == null)
          {
              sKey = "";
          }
          DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
          DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
          DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
          ICryptoTransform desDecrypt = DES.CreateDecryptor();
          byte[] decryptedData = desDecrypt.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

          return decryptedData;
      }
      #endregion BYTES

   }

   sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
   {
       public override Type BindToType(string assemblyName, string typeName)
       {
           Type typeToDeserialize = null;

           String currentAssembly = Assembly.GetExecutingAssembly().FullName;

           // In this case we are always using the current assembly
           assemblyName = currentAssembly;

           // Get the type using the typeName and assemblyName
           typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
               typeName, assemblyName));

           return typeToDeserialize;
       }
   }
  
}      
