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

namespace Machine
{
    /// <summary>
    /// OutputControl.xaml 的交互逻辑
    /// </summary>
    public partial class OutputControl : UserControl
    {
        public OutputControl()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //FB.IsHitTestVisible = false;
            ForceBitDisable();//开始默认不能控制IO
        }

        private void ForceBit_Click(object sender, RoutedEventArgs e)
        {
            if (FB.IsChecked == true)
            {
                ForceBitEnable();
            }
            else ForceBitDisable();

        }
        private void ForceBitEnable()
        {
            //B01.IsHitTestVisible = false;
            foreach (var chlid in this.spIOStatus.Children)
            {
                if (chlid is CheckBox)
                {
                    (chlid as CheckBox).IsHitTestVisible = true;
                }
            }

        }
        private void ForceBitDisable()
        {
            //B01.IsHitTestVisible = true;
            foreach (var chlid in this.spIOStatus.Children)
            {
                if (chlid is CheckBox && (chlid as CheckBox).Name != "FB")
                {
                    (chlid as CheckBox).IsHitTestVisible = false;
                }
            }
        }
    }
}
