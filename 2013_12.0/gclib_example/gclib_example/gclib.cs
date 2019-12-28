using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices; //dll import
using System.IO; //file.exists

#if x86 //defined in "Conditional compilation symbols" of Project Properties
using GReturn = System.Int32;
using GCon = System.IntPtr;
using GSize = System.UInt32;
using GOption = System.Int32;
using GCStringOut = System.Text.StringBuilder;
using GCStringIn = System.String;
using GBufOut = System.Text.StringBuilder;
using GBufIn = System.String;
using GStatus = System.Byte;
// IMPORTANT! Be sure that the paths below are correct 
public static class LibraryPath
{
    public const string GclibDllPath_ = "C:\\Program Files (x86)\\Galil\\gclib\\dll\\x86\\gclib.dll";
    public const string GcliboDllPath_ = "C:\\Program Files (x86)\\Galil\\gclib\\dll\\x86\\gclibO.dll";
}

#elif x64
using GReturn = System.Int32;
using GCon = System.IntPtr;
using GSize = System.UInt32;
using GOption = System.Int32;
using GCStringOut = System.Text.StringBuilder;
using GCStringIn = System.String;
using GBufOut = System.Text.StringBuilder;
using GBufIn = System.String;
using GStatus = System.Byte;
// IMPORTANT! Be sure that the paths below are correct 
public static class LibraryPath
{
    public const string GclibDllPath_ = "C:\\Program Files (x86)\\Galil\\gclib\\dll\\x64\\gclib.dll";
    public const string GcliboDllPath_ = "C:\\Program Files (x86)\\Galil\\gclib\\dll\\x64\\gclibo.dll";
}

#endif

/// <summary>
/// Provides a class that binds to gclib's unmanaged dll. Wraps each call and provides a more user-friendly interface for use in C#.
/// </summary>
/// <remarks>
/// The Gclib class assumes the default installation of gclib, "C:\Program Files (x86)\Galil\gclib\". 
/// If the dlls are elsewhere, change the path strings GclibDllPath_, and GcliboDllPath_.
/// </remarks>
class gclib
{
    #region "C# wrappers of gclib C calls"

    #region "Private properties"
    private const int BufferSize_ = 500000; //size of "char *" buffer. Big enough to fit entire 4000 program via UL/LS, or 24000 elements of array data.
    private GCStringOut Buffer_ = new System.Text.StringBuilder(BufferSize_); //used to pass a "char *" to gclib.
    private byte[] ByteArray_ = new byte[512]; //byte array to hold data record and response to GRead
    private GCon ConnectionHandle_; //keep track of the gclib connection handle.
    #endregion

    /// <summary>
    /// Constructor of the gclib wrapper class.
    /// </summary>
    /// <remarks>Checks to ensure gclib dlls are in the correct location.</remarks>
    /// <exception cref="System.Exception">Will throw an exception if either dll isn't found.</exception>
    public gclib()
    {
        if (!File.Exists(LibraryPath.GclibDllPath_))
            throw new System.Exception("Could not find gclib dll at " + LibraryPath.GclibDllPath_);

        if (!File.Exists(LibraryPath.GcliboDllPath_))
            throw new System.Exception("Could not find gclibo dll at " + LibraryPath.GcliboDllPath_);

    }

    /// <summary>
    /// Return a string array of available connection addresses.
    /// </summary>
    /// <returns>String array containing all available Galil Ethernet controllers, PCI controllers, and COM ports.</returns>
    /// <remarks>Wrapper around gclib GAddresses(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a6a6114683ed5749519b64f19512c24d6
    /// An empty array is returned on error.</remarks>
    public string[] GAddresses()
    {
        GReturn rc = DllGAddresses(Buffer_, BufferSize_);
        if (rc == G_NO_ERROR)
        {
            char[] delimiters = new char[] { '\r', '\n' };
            return Buffer_.ToString().Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
            return new string[0];

    }

    /// <summary>
    /// Downloads array data to a pre-dimensioned array in the controller's array table. 
    /// </summary>
    /// <param name="array_name">String containing the name of the array to download. Must match the array name used in DM.</param>
    /// <param name="data">A list of doubles, to be downloaded.</param>
    /// <param name="first">The first element of the array for sub-array downloads.</param>
    /// <param name="last">The last element of the array for sub-array downloads.</param>
    /// <remarks>Wrapper around gclib GArrayDownload(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#a6ea5ae6d167675e4c27ccfaf2f240f8a 
    /// The array must already exist on the controller, see DM and LA.</remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GArrayDownload(string array_name, ref List<double> data, Int16 first = -1, Int16 last = -1)
    {
        System.Text.StringBuilder ArrayData = new System.Text.StringBuilder(BufferSize_); //for converting to ASCII
        int len = data.Count();
        for (int i = 0; i <= len - 1; i++)
        {
            ArrayData.Append(data[i].ToString("F4")); //format to fixed point
            if (i < len - 1)
            {
                ArrayData.Append(","); //delimiter
            }
        }
        GReturn rc = DllGArrayDownload(ConnectionHandle_, array_name, first, last, ArrayData.ToString());
        if (!(rc == G_NO_ERROR))
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Allows downloading of a program array file to the controller.
    /// </summary>
    /// <param name="Path">The full filepath of the array csv file.</param>
    /// <remarks>Wrapper around gclib GArrayDownload(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a14b448ab8c7e6cf495865af301be398e
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GArrayDownloadFile(string Path)
    {
        GReturn rc = DllGArrayDownloadFile(ConnectionHandle_, Path);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Uploads array data from the controller's array table. 
    /// </summary>
    /// <param name="array_name">String containing the name of the array to upload.</param>
    /// <param name="first">The first element of the array for sub-array uploads.</param>
    /// <param name="last">The last element of the array for sub-array uploads.</param>
    /// <returns>The desired array as a list of doubles.</returns>
    /// <remarks>Wrapper around gclib GArrayUpload(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#af215806ec26ba06ed3f174ebeeafa7a7
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public List<double> GArrayUpload(string array_name, Int16 first = -1, Int16 last = -1)
    {
        List<double> array = new List<double>();
        GReturn rc = DllGArrayUpload(ConnectionHandle_, array_name, first, last, 1, Buffer_, BufferSize_);
        //1 = comma delim
        if (!(rc == G_NO_ERROR))
        {
            throw new System.Exception(GError(rc));
        }
        char[] delimiters = new char[] { ',' };

        string[] tokens = Buffer_.ToString().Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
        double value = 0;
        foreach (string s in tokens)
        {
            if (!double.TryParse(s, out value))
            {
                throw new System.Exception("Could not parse " + s + " into double");
            }
            array.Add(value);
        }
        return array;
    }

    /// <summary>
    /// Allows uploading of a program array file from the controller to an array CSV file.
    /// </summary>
    /// <param name="Path">The full filepath of the array csv file to save.</param>
    /// <param name="Names">A space separated list of the array names to upload. A null string uploads all arrays in the array table (LA).</param>
    /// <remarks>Wrapper around gclib GArrayUpload().
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#af215806ec26ba06ed3f174ebeeafa7a7
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GArrayUploadFile(string Path, string Names)
    {
        GReturn rc = DllGArrayUploadFile(ConnectionHandle_, Path, Names);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Assigns IP address over the Ethernet to a controller at a given MAC address. 
    /// </summary>
    /// <param name="ip">The ip address to assign. The hardware should not yet have an IP address. </param>
    /// <param name="mac">The MAC address of the hardware.</param>
    /// <remarks>Wrapper around gclib GAssign(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#acc996b7c22cfed8e5573d096ef1ab759
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GAssign(string ip, string mac)
    {
        GReturn rc = DllGAssign(ip, mac);
        if (!(rc == G_NO_ERROR))
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Used to close a connection to Galil hardware.
    /// </summary>
    /// <remarks>Wrapper around gclib GClose(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#a24a437bcde9637b0db4b94176563a052
    /// Be sure to call GClose() whenever a connection is finished.</remarks>
    public void GClose()
    {
        DllGClose(ConnectionHandle_);
    }

    /// <summary>
    /// Used for command-and-response transactions.
    /// </summary>
    /// <param name="Command">The command to send to the controller. Do not append a carriage return. Use only ASCII-based commmands.</param>
    /// <param name="Trim">If true, the response will be trimmed of the trailing colon and any leading or trailing whitespace.</param>
    /// <returns>The command's response.</returns>
    /// <remarks>Wrapper around gclib GCommand(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#a5ac031e76efc965affdd73a1bec084a8
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public string GCommand(string Command, bool Trim = true)
    {
        GSize bytes_read = 0;
        GReturn rc = DllGCommand(ConnectionHandle_, Command, Buffer_, BufferSize_, ref bytes_read);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
        if (Trim)
        {
            string r = Buffer_.ToString();
            if (r[r.Count() - 1] == ':')
            {
                r = r.Substring(0, r.Count() - 1);
            }
            return r.Trim();
        }
        else
        {
            return Buffer_.ToString();
        }
    }

    /// <summary>
    /// Provides a human-readable error message from a gclib error code.
    /// </summary>
    /// <param name="ErrorCode">The gclib error code, as returned from a call to the gclib.</param>
    /// <returns>Error message string.</returns>
    /// <remarks>
    /// Wrapper around gclib GError(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#afef1bed615bd72134f3df6d3a5723cba
    ///  This function is private, all public calls that throw errors use this command for setting the exception message.
    /// </remarks>
    private string GError(GReturn ErrorCode)
    {
        DllGError(ErrorCode, Buffer_, BufferSize_);
        return ErrorCode.ToString() + " " + Buffer_.ToString() + "\n";
    }

    /// <summary>
    /// Upgrade firmware. 
    /// </summary>
    /// <param name="filepath ">The full filepath of the firmware hex file.</param>
    /// <remarks>Wrapper around gclib GFirmwareDownload(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#a1878a2285ff17897fa4fb20182ba6fdf
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GFirmwareDownload(string filepath)
    {
        GReturn rc = DllGFirmwareDownload(ConnectionHandle_, filepath);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>Provides a useful connection string.</summary>
    /// <remarks>Wrapper around gclib GInfo(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a08abfcff8a1a85a01987859473167518
    /// </remarks>
    /// <returns>String containing connection information, e.g. "192.168.0.43, DMC4020 Rev 1.2c, 291". A null string indicates an error was returned from the library.</returns>
    public string GInfo()
    {
        GReturn rc = DllGInfo(ConnectionHandle_, Buffer_, BufferSize_);
        if (rc == G_NO_ERROR)
        {
            return Buffer_.ToString();
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// Provides access to PCI and UDP interrupts from the controller. 
    /// </summary>
    /// <returns>The status byte from the controller. Zero will be returned if a status byte is not read.</returns>
    /// <remarks>Wrapper around gclib GInterrupt(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#a5bcf802404a96343e7593d247b67f132
    /// -s ALL or -s EI must be specified in the address argument of GOpen() to receive interrupts.</remarks>
    public byte GInterrupt()
    {
        byte StatusByte = 0;
        GReturn rc = DllGInterrupt(ConnectionHandle_, ref StatusByte);
        if (rc == G_NO_ERROR)
        {
            return StatusByte;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Provides a list of all Galil controllers requesting IP addresses via BOOT-P or DHCP. 
    /// </summary>
    /// <returns>Each line of the returned data will be of the form "model, serial_number, mac". </returns>
    /// <remarks>Wrapper around gclib GIpRequests(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a0afb4c82642a4ef86f997c39a5518952
    /// An empty array is returned on error.
    /// Call will take roughly 5 seconds to return.</remarks>
    public string[] GIpRequests()
    {
        GReturn rc = DllGIpRequests(Buffer_, BufferSize_);
        if (rc == G_NO_ERROR)
        {
            char[] delimiters = new char[] { '\r', '\n' };
            return Buffer_.ToString().Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
            return new string[0];

    }

    /// <summary>
    /// Provides access to unsolicited messages.
    /// </summary>
    /// <returns>String containing all messages received by controller.</returns>
    /// <remarks>Wrapper around gclib GMessage(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#aabc5eaa09ddeca55ab8ee048b916cbcd
    ///An empty string is returned on error.
    /// -s ALL or -s MG must be specified in the address argument of GOpen() to receive messages.</remarks>
    public string GMessage()
    {
        GReturn rc = DllGMessage(ConnectionHandle_, Buffer_, BufferSize_);
        if (rc == G_NO_ERROR)
        {
            return Buffer_.ToString();
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// Blocking call that returns once all axes specified have completed their motion. 
    /// </summary>
    /// <param name="axes">A string containing a multiple-axes mask. Every character in the string should be a valid argument to MG_BGm, i.e. XYZWABCDEFGHST.</param>
    /// <remarks>Wrapper around gclib GMotionComplete(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a19c220879442987970706444197f397a
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GMotionComplete(string axes)
    {
        GReturn rc = DllGMotionComplete(ConnectionHandle_, axes);
        if (!(rc == G_NO_ERROR))
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Used to open a connection to Galil hardware.
    /// </summary>
    /// <param name="address">Address string including any connection switches. See gclib documentation for GOpen().</param>
    /// <remarks>Wrapper around gclib GOpen(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#aef4aec8a85630eed029b7a46aea7db54
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GOpen(string address)
    {
        GReturn rc = DllGOpen(address, ref ConnectionHandle_);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Allows downloading of a DMC program from a string buffer.
    /// </summary>
    /// <param name="program">The program to download.</param>
    /// <param name="preprocessor">Preprocessor directives. Use nullstring for none.</param>
    /// <remarks>Wrapper around gclib GProgramDownload(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#acafe19b2dd0537ff458e3c8afe3acfeb
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GProgramDownload(string program, string preprocessor = "")
    {
        GReturn rc = DllGProgramDownload(ConnectionHandle_, program, preprocessor);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Allows downloading of a DMC program from file.
    /// </summary>
    /// <param name="file_path">The full filepath of the DMC file.</param>
    /// <param name="preprocessor">Preprocessor directives. Use nullstring for none.</param>
    /// <remarks>Wrapper around gclib GProgramDownloadFile(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a8e44e2e321df9e7b8c538bf2d640633f
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GProgramDownloadFile(string file_path, string preprocessor = "")
    {
        GReturn rc = DllGProgramDownloadFile(ConnectionHandle_, file_path, preprocessor);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Allows uploading of a DMC program to a string.
    /// </summary>
    /// <remarks>Wrapper around gclib GProgramUpload(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#a80a653ce387a2bd16bde2793c6de77e9
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public string GProgramUpload()
    {
        GReturn rc = DllGProgramUpload(ConnectionHandle_, Buffer_, BufferSize_);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
        else
        {
            return Buffer_.ToString();
        }
    }

    /// <summary>
    /// Allows uploading of a DMC program to a file.
    /// </summary>
    /// <param name="file_path">The full filepath of the DMC file to save.</param>
    /// <remarks>Wrapper around gclib GProgramUploadFile(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a38c5565afc11762fa19d37fbaa3c9aa3
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GProgramUploadFile(string file_path)
    {
        GReturn rc = DllGProgramUploadFile(ConnectionHandle_, file_path);
        if (rc != G_NO_ERROR)
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Performs a read on the connection. 
    /// </summary>
    /// <returns>String containing the read data, or a nullstring if nothing was read or an error occured.</returns>
    /// <remarks>Wrapper around gclib GRead(), 
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#adab6ec79b7e1bc7f0266684dd3434923
    /// </remarks>
    public byte[] GRead()
    {
        GSize read = 0;
        GReturn rc = DllGRead(ConnectionHandle_, ByteArray_, (uint)ByteArray_.Length, ref read);
        if (rc == G_NO_ERROR)
        {
            byte[] ReturnData = new byte[read];
            //create an array of the correct size
            for (GSize i = 0; i <= read - 1; i++)
            {
                ReturnData[i] = ByteArray_[i];
                //copy over the data
            }
            return ReturnData;
        }
        else
            return new byte[0];
    }

    /// <summary>
    /// Used for retrieving data records from the controller.
    /// </summary>
    /// <returns>A byte array containing the data record.</returns>
    /// <param name="async">False to user QR, True to use DR.</param>
    /// <remarks>Wrapper around gclib GRecord(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#a1f39cd57dcfa55d065c972a020b1f8ee
    /// To use async, -s ALL or -s DR must be specified in the address argument of GOpen(),
    /// and the records must be started via DR or RecordRate().
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public byte[] GRecord(bool async)
    {
        ushort method = 0;
        if (async)
            method = 1;

        GReturn rc = DllGRecord(ConnectionHandle_, ByteArray_, method);
        if (rc != G_NO_ERROR)
            throw new System.Exception(GError(rc));

        return ByteArray_;
    }

    /// <summary>
    /// Sets the asynchronous data record to a user-specified period via DR. 
    /// </summary>
    /// <param name="period_ms">Period, in milliseconds, to set up for the asynchronous data record.</param>
    /// <remarks>Wrapper around gclib GRecordRate(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#ada86dc9d33ac961412583881963a1b8a
    /// Takes TM and product type into account and sets the DR period to the period requested by the user, if possible.</remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GRecordRate(double period_ms)
    {
        GReturn rc = DllGRecordRate(ConnectionHandle_, period_ms);
        if (!(rc == G_NO_ERROR))
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Set the timeout of communication transactions. Use -1 to set the original timeout from GOpen().
    /// </summary>
    /// <param name="timeout_ms ">New timeout in miliseconds.</param>
    /// <remarks>Wrapper around gclib GTimeout(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a179aa2d1b8e2227944cc06a7ceaf5640
    /// </remarks>
    public void GTimeout(Int16 timeout_ms)
    {
        DllGTimeout(ConnectionHandle_, timeout_ms);
    }

    /// <summary>Used to get the gclib version.</summary>
    /// <returns>The library version, e.g. "104.73.179". A null string indicates an error was returned from the library.</returns>
    /// <remarks>Wrapper around gclib GVersion(),
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclibo_8h.html#a1784b39416b77af20efc98a05f8ce475
    /// </remarks>
    public string GVersion()
    {
        GReturn rc = DllGVersion(Buffer_, BufferSize_);
        if (rc == G_NO_ERROR)
        {
            return Buffer_.ToString();
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// Performs a write on the connection. 
    /// </summary>
    /// <param name="buffer">The user's write buffer. To send a Galil command, a terminating carriage return is usually required. </param>
    /// <remarks>Wrapper around gclib GWrite(), 
    /// http://www.galil.com/sw/pub/all/doc/gclib/html/gclib_8h.html#abe28ebaecd5b3940adf4e145d40e5456 
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public void GWrite(string buffer)
    {
        GReturn rc = DllGWrite(ConnectionHandle_, buffer, (uint) buffer.Length);
        if (!(rc == G_NO_ERROR))
        {
            throw new System.Exception(GError(rc));
        }
    }

    /// <summary>
    /// Allows downloading of a Galil compressed backup (gcb) file to the controller.
    /// </summary>
    /// <param path="Path">The full filepath of the gcb file.</param>
    /// <param options="Options">A bit mask indicating which sectors of the gcb file to restore to the controller.</param>
    /// <returns>The controller information stored in the gcb file.</returns>
    /// <remarks>Wrapper around gclib GSetupDownloadFile(),
    /// 
    /// If options is specified as 0, the return string will have a number appended corresponding to a bit mask of the available gcb sectors
    /// </remarks>
    /// <exception cref="System.Exception">Will throw an exception if anything other than G_NO_ERROR is received from gclib.</exception>
    public string[] GSetupDownloadFile(string path, Int32 options)
	{
		GReturn rc = DllGSetupDownloadFile(ConnectionHandle_, path, options, Buffer_, BufferSize_);

		string ret_buf = Buffer_.ToString();
        ret_buf = ret_buf.Replace("\r\n", ", ");
		
		if (options != 0)
		{
			if (rc != G_NO_ERROR)
			{
				throw new System.Exception(GError(rc));
			}
		}
		else
		{
			ret_buf += "\"options\"," + rc + "\n";
		}

        char[] delimiters = new char[] { '\n' };
        return ret_buf.ToString().Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
	}

    #endregion

    #region "DLL Imports"
    //Import declarations for gclib functions. Functions are private to this class and are prefixed with "Dll" to distinguish from C# functions.

    #region "Error Codes"
    /// <summary>
    /// Functions are checked for G_NO_ERROR.<
    /// </summary>
    /// <remarks>Some functions throw exceptions if G_NO_ERROR is not returned.</remarks>
    private const Int32 G_NO_ERROR = 0;
    #endregion

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GAddresses", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGAddresses(GCStringOut addresses, GSize addresses_len);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GArrayDownload", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGArrayDownload(GCon g, GCStringIn array_name, GOption first, GOption last, GCStringIn buffer);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GArrayDownloadFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGArrayDownloadFile(GCon g, GCStringIn path);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GArrayUpload", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGArrayUpload(GCon g, GCStringIn array_name, GOption first, GOption last, GOption delim, GCStringOut buffer, GSize bufferLength);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GArrayUploadFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGArrayUploadFile(GCon g, GCStringIn path, GCStringIn names);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GAssign", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGAssign(GCStringIn ip, GCStringIn mac);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GClose", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGClose(GCon g);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GCommand", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGCommand(GCon g, GCStringIn command, GCStringOut buffer, GSize bufferLength, ref GSize bytesReturned);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GError", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void DllGError(GReturn error_code, GCStringOut errorbuf, GSize error_len);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GFirmwareDownload", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGFirmwareDownload(GCon g, GCStringIn path);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GInfo", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGInfo(GCon g, GCStringOut info, GSize infoLength);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GInterrupt", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGInterrupt(GCon g, ref GStatus status_byte);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GIpRequests", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGIpRequests(GCStringOut requests, GSize requests_len);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GMessage", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGMessage(GCon g, GCStringOut buffer, GSize bufferLength);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GMotionComplete", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGMotionComplete(GCon g, GCStringIn axes);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GOpen", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGOpen(GCStringIn address, ref GCon g);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GProgramDownload", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGProgramDownload(GCon g, GCStringIn program, GCStringIn preprocessor);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GProgramDownloadFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGProgramDownloadFile(GCon g, GCStringIn path, GCStringIn preprocessor);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GProgramUpload", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGProgramUpload(GCon g, GCStringOut buffer, GSize bufferLength);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GProgramUploadFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGProgramUploadFile(GCon g, GCStringIn path);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GRead", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGRead(GCon g, byte[] record, GSize buffer_len, ref GSize bytes_read);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GRecord", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGRecord(GCon g, byte[] record, GOption method);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GRecordRate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGRecordRate(GCon g, double period_ms);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GTimeout", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void DllGTimeout(GCon g, GOption timeoutMs);

    [DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGVersion(GCStringOut ver, GSize ver_len);

    [DllImport(LibraryPath.GclibDllPath_, EntryPoint = "GWrite", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGWrite(GCon g, GCStringIn buffer, GSize buffer_len);
	
	[DllImport(LibraryPath.GcliboDllPath_, EntryPoint = "GSetupDownloadFile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern GReturn DllGSetupDownloadFile(GCon g, GCStringIn file_path, GOption options, GCStringOut info, GSize info_len);

    #endregion
} //gclib class

