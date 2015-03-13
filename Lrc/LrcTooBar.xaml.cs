﻿using System;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Lrc
{
    /// <summary>
    /// LrcTooBar.xaml 的交互逻辑
    /// </summary>
    public partial class LrcTooBar : Window
    {   

        public DispatcherTimer dt = new DispatcherTimer();
        public LrcTooBar()
        {
            InitializeComponent();
            base.Left = (SystemParameters.PrimaryScreenWidth - base.Width) / 2.0;
            base.Top = (SystemParameters.PrimaryScreenHeight - base.Height) - 50.0;
            this.dt.Interval = TimeSpan.FromMilliseconds(3000.0);
            this.dt.Tick += new EventHandler(this.dt_Tick);
        }

        private void btnPlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (btnPlay.ToolTip.ToString() != "播放")
            {
                LrcController.SetButtonChanged(sender, 2);
                LrcController.setPlay();
            }
            else
            {
                LrcController.SetButtonChanged(sender, 3);
                LrcController.setPause();
            }
        }

        private void dt_Tick(object sender, EventArgs e)
        {
            base.Hide();
            this.dt.Stop();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LrcController.SetButtonChanged(sender, 0);
        }
    }
}
