using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Panuon.UI.Silver;
using System.Windows.Threading;
using Utility;

namespace Machine
{
    /// <summary>
    /// GalilControlWPF.xaml 的交互逻辑
    /// </summary>
    public partial class GalilControlWPF : UserControl
    {
        public GalilControlWPF()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 退出GUI时的操作
        /// </summary>
        public void GalilControl_Unloaded()
        {
            LogServiceHelper.Intance.FocrceWriteTOFile = true; //把为写入文件的记录，写入文件            
            this.hWGalil.Disconnect();     //断开galil            
            Thread thread;
            thread = new Thread(new ThreadStart(() => {
                Thread.Sleep(5000);
                LogServiceHelper.Intance.Stop();
            }));// 停止记录数据
        }

        public HWGalil hWGalil = HWDevces.HWGalil1;
        Nest nest12 =HWDevces.Nest12;
        
        Nest nest34 = HWDevces.Nest34;
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.nest12.PropertyChanged -= OnMessageChange;  //初始化会load两次，所以在这里减一次，避免重复通知
            this.nest12.PropertyChanged += OnMessageChange;
            this.ControlGalil1Input0.FB.Visibility = Visibility.Collapsed;
            this.gridGalil.DataContext = this.hWGalil;
            this.ControlGalil1IO.DataContext = this.hWGalil.IOOutput0Exp;
            this.ControlGalil1IO.lbIOActual.DataContext = this.hWGalil.IOOutput0Act;
            this.ControlGalil1Input0.DataContext = this.hWGalil.IOInput0;
            this.ControlGalil1Input0.lbIOActual.Visibility = Visibility.Hidden;
            this.ContolGalil1Input0Force.DataContext = this.hWGalil.IOInput0;
            this.ControlGalil1Input0.lbIOActual.DataContext = this.hWGalil.IOInput0;

            string dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log/");
            LogServiceHelper.Intance.Start(System.IO.Path.Combine(dir));
            this.hWGalil.PropertyChanged -= this.OnLogMessageChange; //初始化会load两次，所以在这里减一次，避免重复通知
            this.hWGalil.PropertyChanged += this.OnLogMessageChange;
            //dgMsg.Columns[0].HeaderStringFormat = "HH:mm:ss fff";
            dgMsg.ItemsSource = LogServiceHelper.Intance.LogModelInformations;            
            var lockObj = new object();
            BindingOperations.EnableCollectionSynchronization(LogServiceHelper.Intance.LogModelInformations, lockObj);
           
            
            //HWDevces.Messages = new List<Message>();


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.hWGalil.IsConnected)
            {

                this.lbGalilInf.Content = this.hWGalil.Disconnect();
                this.btGalilConnect.Content = "Disconnected";
                // this.btGalilConnect.Background = ;
            }
            else
            {
                this.lbGalilInf.Content = hWGalil.Connect();
                if (this.lbGalilInf.Content.ToString().Contains("ERROR")) return;
                //this.hWGalil.DigitalO0Exp = 180;
                this.btGalilConnect.Content = "Connected";
                //this.btGalilConnect.BackColor = Color.Green;

            }
        }

        private void BtSendCmd_Click(object sender, RoutedEventArgs e)
        {
            if (tbCmd.Text.Length > 0)
            {
                this.hWGalil.SendCommand(tbCmd.Text, true);
            }
        }

        private void TbCmd_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.hWGalil.SendCommand(tbCmd.Text, true);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (HWDevces.Messages == null) HWDevces.Messages = new List<Message>();
            HWDevces.Messages.Add(new Message(MessageType.Worning, "d", "t"));
            
            if (!nest12.Idle) Notice.Show(DateTime.Now.ToString() + ":\n"+ nest12.NestType.ToString() + ";正在忙，请稍后再试", "北京交通局温馨提示", 5);
            Thread thread;
            if (nest12.NestIsUnSnug())
                thread = new Thread(new ThreadStart(()=> { nest12.NestSnug(); }));//创建线程
            else
                thread = new Thread(new ThreadStart(() => { nest12.NestUnSnug();}));//创建线程

            thread.Start(); 
        }
        private void OnMessageChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Notice.Show(DateTime.Now.ToString() + ":\n轴不在0点，请先检查确保无问题", "北京交通局温馨提示", 5);
            
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {                ;
                tbRspImpt.Text += "\n" + nest34.Message;
            }));
        }
        private void OnLogMessageChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //return;
            //Notice.Show(DateTime.Now.ToString() + ":\n轴不在0点，请先检查确保无问题", "北京交通局温馨提示", 5);
            if (sender is HWGalil && e.PropertyName == "MessageIpt") return;
            //Thread thread = new Thread(() => {
            string msg = (string)sender.GetType().GetProperty(e.PropertyName).GetValue(sender, null);
            LogServiceHelper.Intance.Write("信息", msg, (string)sender.GetType().GetProperty("IPAdress").GetValue(sender, null) + ": " + sender + "." + e.PropertyName);
            //});
            //thread.Name = "test";
            //thread.Priority = ThreadPriority.Lowest;
            //thread.IsBackground = true;
            //thread.Start();
        }

        private void dgMsg_Unloaded(object sender, RoutedEventArgs e)
        {
            ; dgMsg.ItemsSource = null;
        }
    }
}
