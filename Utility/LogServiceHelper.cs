﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;


namespace Utility
{
    /**
     * 日志写入组件(**多线程写日志队列，单线程写日志文件**)
     * created by 王军华 20170207  599871338
     * 使用方法：
     *          在Global.asax的Application_Start方法中调用： Utility.LogServiceHelper.Intance.Start();
     *          在使用的地方调用 LogServiceHelper.Intance.PutLog(new LogModel() { CreatedTime=DateTime.Now, Dir="operator", Msg=log.URL, Operator=log.LoginName });
     *          在关闭的地方调用LogServiceHelper.Intance.Stop();
     * 功能描述：1.多类型日志自动归类，将不同类型的日志写入不同的文件中。例如：登录日志，操作日志，错误日志等
     *           2.批量写入日志，减少IO操作   
     *           3.延时写入功能，当日志大小小于某个阈值时不写入
     *           4.强制写入功能，当日志在批处理列表中保留时间超过某个阈值时强制写入
     *           5.日志分割(按天，按体积分割)
     * */
    public class LogServiceHelper : INotifyPropertyChanged
    {
        private LogServiceHelper()
        {
            logModelMessages = new ObservableCollection<LogModel>();
            LogModelErrors = new ObservableCollection<LogModel>();
            LogModelErrors = new ObservableCollection<LogModel>();
            LogModelCommunications = new ObservableCollection<LogModel>();
        }
        public static readonly LogServiceHelper Intance = new LogServiceHelper();
        private bool _isStart = false;
        private Thread _thread = null;
        private int _maxByteSize = 1024 * 64;//64KB,当单个缓存日志达到多少byte时写入文件
        private int _maxFileSize = 1024 * 1024 * 1;//1MB,单个日志文件的大小
        private int _maxSecond = 30;//最大相隔时间，不管缓存区中的日志大小到不到64K都写入硬盘
        private int _maxWriteErrorTime = 10;//最大连续写入错误次数
        private int _currentWriteErrorTime = 0;//当前连续写入错误次数
        private string _baseDirectory = string.Empty;
        private LogModel _tmpLog = null;
        private ObservableCollection<LogModel> logModelMessages;  //ObservableCollection 允许多线程访问
        private ObservableCollection<LogModel> logModelErrors;  //ObservableCollection 允许多线程访问
        private ObservableCollection<LogModel> logModelWornings;  //ObservableCollection 允许多线程访问
        private ObservableCollection<LogModel> logModelCommunications;  //ObservableCollection 允许多线程访问

        private ConcurrentQueue<LogModel> queue = new ConcurrentQueue<LogModel>();//日志队列      
        //对不同类型的日志进行区分合并
        private List<LogBatch> _logBatch = new List<LogBatch>();//批处理列表
        private LogBatch _tmpLogBatch = null;

        public bool FocrceWriteTOFile = false;
        public ObservableCollection<LogModel> LogModelInformations { get => logModelMessages; private set => logModelMessages = value; }      
        public ObservableCollection<LogModel> LogModelErrors { get => logModelErrors; private set => logModelErrors = value; }
        public ObservableCollection<LogModel> LogModelWornings { get => logModelWornings; private set => logModelWornings = value; }
        public ObservableCollection<LogModel> LogModelCommunications { get => logModelCommunications; private set => logModelCommunications = value; }

        /// <summary>
        /// 开始写入日志，系统初始化时调用：向磁盘写入日志需要满足同一类型日志条数>=batchCount条或者同一类型日志存在超过maxSecond秒
        /// </summary>
        /// <param name="baseDirectory">日志保存的基目录(全路径)，例如：System.Web.HttpContext.Current.Server.MapPath("~/logs/")</param>
        /// <param name="maxByteSize">当日志缓存队列日志达到多少byte时写入文件,默认64KB</param>
        /// <param name="maxFileSize">单个日志文件大小，默认1M</param>
        /// <param name="maxSecond">当同一类型日志存在且超过maxSecond秒时向磁盘写入日志</param>
        public void Start(string baseDirectory, int maxByteSize = 65536, int maxFileSize = 1048576, int maxSecond = 30)
        {
            if (_isStart) return;
            this._baseDirectory = baseDirectory;
            this._maxByteSize = maxByteSize;
            this._maxFileSize = maxFileSize;
            this._maxSecond = maxSecond;
            _thread = new Thread(TastExecuting);
            _thread.IsBackground = true;
            _thread.Name = "LogService";
            _thread.Start();
        }
        public void AgainStart()
        {
            _isStart = true;
        }
        public void Stop()
        {
            _isStart = false;
        }
        private void TastExecuting()
        {
            _isStart = true;
            while (_isStart)
            {

                while (queue.Count > 0)
                {
                    QueueOut();
                    LogBatchWrite();
                }
                Thread.Sleep(100);
                LogBatchWrite();

            }
        }
        public long GetQueueCount()
        {
            return queue.Count;
        }
        private void QueueOut()
        {
            queue.TryDequeue(out _tmpLog);
            if (_tmpLog != null)
            {
                //不同类型的日志放在不同的对象中
                _tmpLogBatch = _logBatch.FirstOrDefault(c => c.Dir == _tmpLog.Dir);
                if (_tmpLogBatch == null)
                {
                    _tmpLogBatch = new LogBatch();
                    _tmpLogBatch.StartTime = DateTime.Now;
                    _tmpLogBatch.Dir = _tmpLog.Dir;
                    _logBatch.Add(_tmpLogBatch);
                }
                _tmpLogBatch.Count += 1;
                _tmpLogBatch.LogStr = _tmpLogBatch.LogStr.AppendFormat("时间【{0}】，严重性【{1}】，操作人【{2}】，信息【{3}】" + Environment.NewLine, _tmpLog.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss fff", System.Globalization.CultureInfo.CurrentCulture),_tmpLog.Level, _tmpLog.Operator, _tmpLog.Message);
                _tmpLog = null;
            }
        }
        private void LogBatchWrite()
        {
            //缓存日志，定期写入文件，空间换时间 避免频繁操作硬盘 造成IO开销过大       
            foreach (var item in _logBatch)
            {
                int second = (DateTime.Now - item.StartTime).Seconds;
                if (item.LogStr.Length > _maxByteSize || second >= _maxSecond || FocrceWriteTOFile)
                {
                    //写入日志
                    if (WriteFile(item.LogStr.ToString(), item.Dir))
                    {
                        item.StartTime = DateTime.Now;
                        item.Count = 0;
                        item.LogStr.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        public void Abort()
        {
            _isStart = false;
            _thread.Abort();
        }
        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="dir">日志类型，如登录日志(log)，错误日志(error)，操作日志(operator)</param>
        /// <param name="content">日志内容</param>
        /// <param name="userName">操作人</param>
        public void Write(string logType, string content, string userName = "")
        {
            Write(new LogModel() { Dir = logType, Message = content, Operator = userName });
        }
        public void Write(LogModel log)
        {
            lock (Intance)
            {
                if (log.Level == LogModel.logLevel.Information)
                {
                    if (this.LogModelInformations.Count > 500) this.LogModelInformations.Remove(this.LogModelInformations.Last());
                    this.LogModelInformations.Insert(0, log);
                }
                else if (log.Level == LogModel.logLevel.Communication)
                {
                    if (this.LogModelCommunications.Count > 1000) this.LogModelCommunications.Remove(this.LogModelCommunications.Last());
                    this.LogModelCommunications.Insert(0, log);
                }
                else if (log.Level == LogModel.logLevel.ERROR) 
                {
                    if (this.LogModelErrors.Count > 50) this.LogModelErrors.Remove(this.LogModelErrors.Last());
                    this.LogModelErrors.Insert(0, log);
                }
                else if (log.Level == LogModel.logLevel.Warning) 
                {
                    if (this.LogModelWornings.Count > 50) this.LogModelWornings.Remove(this.LogModelWornings.Last());
                    this.LogModelWornings.Insert(0, log);
                }
            }
            NotifyPropertyChanged("LogModels");
            queue.Enqueue(log);
        }
        private bool WriteFile(string log, string sDirectory)
        {
            if (string.IsNullOrEmpty(log))
            {
                return true;
            }
            //c:\log\login\2016\1\20160111.log
            DateTime now = DateTime.Now;
            string year = now.Year.ToString("F0", System.Globalization.CultureInfo.CurrentCulture);
            string month = now.Month.ToString("F0", System.Globalization.CultureInfo.CurrentCulture);
            string dir = Path.Combine(_baseDirectory, year, month);
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir); //如果文件夹不存在，则创建一个新的
                }
                else
                {

                    //request_2017年02月13日(0).log
                    //获取文件夹dir下的所有文件
                    int index = 0;
                    var fileList = Directory.GetFiles(dir).ToList();
                    if (fileList.Count > 0)
                    {
                        index = fileList.Count(c => c.Contains(sDirectory));
                        if (index > 0)
                        {
                            index--;
                        }
                    }
                    string filename = sDirectory + "_" + now.ToString("yyyy年MM月dd日", System.Globalization.CultureInfo.CurrentCulture) + "(" + index + ").log";
                    string fileNameNew = Path.Combine(dir, filename); //文件不存在则创建
                    if (!System.IO.File.Exists(fileNameNew))
                    {
                        WriteFile(log, fileNameNew, true);
                    }
                    else
                    {
                        //获取文件大小
                        FileInfo fileInfo = new FileInfo(fileNameNew);
                        if (fileInfo.Length > _maxFileSize)//如果大于单个文件最大体积
                        {
                            fileNameNew = fileNameNew.Replace("(" + index + ").log", "(" + (index + 1) + ").log");
                            if (!System.IO.File.Exists(fileNameNew))
                            {
                                WriteFile(log, fileNameNew, true);
                            }
                            else
                            {
                                WriteFile(log, fileNameNew, false);
                            }
                        }
                        else
                        {

                            WriteFile(log, fileNameNew, false);
                        }

                    }

                }
                _currentWriteErrorTime = 0;
                return true;
            }
            catch (Exception ex)
            {
                _currentWriteErrorTime++;
                Thread.Sleep(10000);//暂停10秒
                if (_currentWriteErrorTime > _maxWriteErrorTime)
                {
                    Stop();//停止写入日志，发送报警短信
                }
                return false;
            }
            finally
            {

            }

        }
        private void WriteFile(string log, string filePath, bool isCreated)
        {
            FileStream file = null;
            if (isCreated)
            {
                file = new FileStream(filePath, FileMode.CreateNew);
            }
            else
            {
                file = new FileStream(filePath, FileMode.Append);
            }
            StreamWriter sw = new StreamWriter(file, Encoding.Default);
            sw.Write(log);
            sw.Flush();
            sw.Close();
            file.Close();
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    [Serializable]
    public class LogModel : INotifyPropertyChanged
    {
        private DateTime createdTime;
        private logLevel level;
        private string @operator;
        private string message;

        public enum logLevel
        {
            Communication,
            Information,
            Warning,
            ERROR,
        }


        public LogModel()
        {
            CreatedTime = DateTime.Now;
            Level = logLevel.Information;

        }

        public LogModel(string msg, string @operator, logLevel level, string dir, DateTime createdTime)
        {
            CreatedTime = createdTime;
            Message = msg ?? throw new ArgumentNullException(nameof(msg));
            Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
            Level = level;
            Dir = dir ?? throw new ArgumentNullException(nameof(dir));
        }

        public DateTime CreatedTime { get => createdTime; private set { createdTime = value; } }
        public int Millisecond { get => createdTime.Millisecond; }
        public logLevel Level { get => level; private set { level = value; }
}
        public string Operator { get => @operator; set { @operator = value;  } }
        public string Message { get => message; set { message = value;  } }


        /// <summary>
        /// 保存的文件夹，基础文件夹为项目所在位置下的/log，如果Dir为login，那么Dir=/log/login
        /// </summary>
        public string Dir { get; set; }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    [Serializable]
    internal class LogBatch
    {
        public LogBatch()
        {
            LogStr = new StringBuilder();
        }
        public string Dir { get; set; }
        public int Count { get; set; }
        public DateTime StartTime { get; set; }
        public StringBuilder LogStr { get; set; }
    }

}
