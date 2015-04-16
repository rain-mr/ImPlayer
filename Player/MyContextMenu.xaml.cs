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
using Lyrics;
using Player.Setting;

namespace Player
{
    /// <summary>
    /// DiyContextMenu.xaml 的交互逻辑
    /// </summary>
    public partial class DiyContextMenu : Window
    {
        public DiyContextMenu()
        {
            InitializeComponent();
            btnPlay.Style = PlayController.bassEng.IsPlaying ? (Style)this.FindResource("pause") : (Style)this.FindResource("play");
            btnMute.Style = PlayController.bassEng.IsMuted ? (Style)this.FindResource("Mute") : (Style)this.FindResource("notMute");

        }

        private void btnPre_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() => {
                PlayController.PlayPrevent();
            }));     
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {


            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (PlayController.bassEng.IsPlaying)
                {
                    PlayController.Pause();
                    btnPlay.Style = (Style)this.FindResource("play");
                }
                else
                {
                    if (PlayController.bassEng.CanPlay)
                        PlayController.Play();
                    else
                        PlayController.PlayMusic();
                    btnPlay.Style = (Style)this.FindResource("pause");
                }
            }));
            
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                PlayController.PlayNext();
            }));
            
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            AppPropertys.setFreeNotifyIcon();
            //Environment.Exit(0);
            Application.Current.Shutdown(-1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppPropertys.SetLrcShow();
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            PlayController.setMute();
            btnMute.Style = PlayController.bassEng.IsMuted ? (Style)this.FindResource("Mute") : (Style)this.FindResource("notMute");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindingOperations.SetBinding(soundSlider, Slider.ValueProperty,
                         new Binding
                         {
                             Source = PlayController.bassEng,
                             Path = new PropertyPath("Volume"),
                             Mode = BindingMode.TwoWay
                         });
        }
        SettingPage setingPage = null;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (setingPage == null || !setingPage.IsLoaded)
                    setingPage = new SettingPage(AppPropertys.HotKeys);
                setingPage.SettingReloadHandler += new SettingReloadDelegate(AppPropertys.mainWindow.SaveConfig);
                setingPage.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                setingPage.Show();
            }));
           
        }

        
    }
}