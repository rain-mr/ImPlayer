﻿using Player.FileTypeAssocion;
using Player.HotKey;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Player.Setting
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public delegate void SettingReloadDelegate();
    public partial class SettingPage : Window
    {
        /// <summary>
        /// 热键设置，关闭窗口前为默认设置，关闭窗口后更新为新设置
        /// </summary>
        internal HotKeys HotKeys { get; private set; }
        public event SettingReloadDelegate SettingReloadHandler;
        private ObservableCollection<DataItem> items = new ObservableCollection<DataItem>();

        public ObservableCollection<DataItem> Items
        {
            get { return items; }
            set { items = value; }
        }
        public SettingPage(HotKeys hotKeys)
        {
            #region FileType Init
            Items.Add(new DataItem() { Name = "AAC 音频文件(.aac)" });
            Items.Add(new DataItem() { Name = "MP4 音频文件(.m4a)" });
            Items.Add(new DataItem() { Name = "MP3 音频文件(.mp3)" });
            Items.Add(new DataItem() { Name = "Money's Audio 音频文件(.ape)" });
            Items.Add(new DataItem() { Name = "FLAC音频文件(.flac)" });
            Items.Add(new DataItem() { Name = "Windows Media 音频文件(.wma)" });
            Items.Add(new DataItem() { Name = "Wave Audio 音频文件(.wav)" });
            Items.Add(new DataItem() { Name = "Voribs/OGG 音频文件(.ogg)" });
            #endregion
            InitializeComponent();
            #region 读取热键
            HotKeys = hotKeys;
            if (hotKeys == null) { return; }
            foreach (var child in this.yy.Children)
            {
                if (child is HotKeySettingControl)
                {
                    HotKeySettingControl setting = child as HotKeySettingControl;
                    if (hotKeys.ContainsKey(setting.Command))
                        setting.HotKey = hotKeys[setting.Command];
                }
            }
            #endregion
            
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            #region 保存热键
            //HotKeys.Clear();
            //foreach (var child in this.yy.Children)
            //{
            //    if (child is HotKeySettingControl)
            //    {
            //        HotKeySettingControl setting = child as HotKeySettingControl;
            //        if (setting.HotKey != null)
            //            HotKeys.Add(setting.Command, setting.HotKey);
            //    }
            //}
            #endregion

            #region 类型关联
            List<string> fileTypes=new List<string>();
            //foreach (DataItem d in Items)
            //{
            //    if (d.IsEnabled) { fileTypes.Add(d.Name.Substring(d.Name.IndexOf('(').TrimEnd(')'))); }
            //}
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            fileTypes.AddRange(Items.Where(item => item.IsEnabled).Select(ss => ss.Name.Substring(ss.Name.IndexOf('(')+1).Trim(')')));
            TypeRegsiter.Regsiter(dir + "\\Player.exe",dir + "\\Symbian_Anna.dll", fileTypes);
          //  TypeRegsiter.Regsiter(fileTypes);  //TODO
            #endregion
            this.Close();

            if (SettingReloadHandler != null)
            {
                SettingReloadHandler();
            }
        }

        private void playControlKey_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAll.IsChecked = true;
        }

        private void btnNoSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAll.IsChecked = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        public static string GetCurrentVersion()
        {
           return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}