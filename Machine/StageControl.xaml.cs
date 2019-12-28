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
using Machine.Helpers;

namespace Machine
{
    /// <summary>
    /// StageControl.xaml 的交互逻辑
    /// </summary>
    public partial class StageControl : UserControl
    {
        public StageControl()
        {
            InitializeComponent();
        }
        AxisSimulator axisSimulator;
        bool stopMotor;

        private void JogLeft_Click(object sender, RoutedEventArgs e)
        {
            float step;
            if (float.TryParse(tbJogStep.Text,out step))            
                axisSimulator.JogReference(-step);
        }

        private void JogRight_Click(object sender, RoutedEventArgs e)
        {
            float step;
            if (float.TryParse(tbJogStep.Text, out step))
                axisSimulator.JogReference(step);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            axisSimulator.MoveAbsolute(0F, 250f);
        }

        private void UnLoad_Click(object sender, RoutedEventArgs e)
        {
            axisSimulator.MoveAbsolute(450f, 250f);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //SldCustom.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.Both;
            //UpdateTemplate();
            //SliderHelper.SetIsTickValueVisible(SldCustom, true);
            //UpdateCode();
            axisSimulator = new AxisSimulator(AxisID.E);
            this.StageE.DataContext = axisSimulator;
        }

        private void ContinueRun_Click(object sender, RoutedEventArgs e)
        {
            if (axisSimulator.PositionCurrent > 0.01)
            {
                Notice.Show(DateTime.Now.ToString() + ":\n轴不在0点，请先检查确保无问题", "北京交通局温馨提示", 5);
                return;
            }
            int runCount = 6;
            Thread thread = new Thread(() => {
                for (int i = 0; i < runCount; i++)
                {                    
                    axisSimulator.MoveAbsolute(450f, 250f);
                    Thread.Sleep(20);
                    while (!axisSimulator.Idle) { Thread.Sleep(5); }
                    axisSimulator.MoveAbsolute(0f, 1000f);
                    Thread.Sleep(20);
                    while (!axisSimulator.Idle) { Thread.Sleep(5); }
                    if(this.stopMotor)
                    {
                        this.stopMotor = false;
                        break;
                    }
                }
                
            });
            thread.Name = "ContinueRun";
            thread.Priority = ThreadPriority.AboveNormal;
            thread.IsBackground = true;
            thread.Start();
            //axisSimulator.MoveAbsolute(450f, 250f);
            //axisSimulator.MoveAbsolute(0f, 1000f);

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            stopMotor = true;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("[^0-9.]+");
            if (tbJogStep.Text.Contains(".") && e.Text == ".") { e.Handled = true; return; }
            e.Handled = re.IsMatch(e.Text);
        }

        private void TextBox_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        #region Function

        private void UpdateTemplate()
        {
            SliderHelper.SetTrackThickness(SldCustom, 10);
            SliderHelper.SetThumbSize(SldCustom, 10);
        }

        private void UpdateCode()
        {
            var style = SliderHelper.GetSliderStyle(SldCustom);
            var thumbSize = SliderHelper.GetThumbSize(SldCustom);
            var trackThickness = SliderHelper.GetTrackThickness(SldCustom);
            var valueVisible = SliderHelper.GetIsTickValueVisible(SldCustom);
            var tickBarVisible = SldCustom.TickPlacement == System.Windows.Controls.Primitives.TickPlacement.Both;
        }


        #endregion

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }
    }
}
