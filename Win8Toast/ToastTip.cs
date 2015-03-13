﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using Win8Toast.ShellHelpers;

namespace Win8Toast
{
    class ToastTip
    {
        private const String APP_ID = "CJ Player";
        #region
        public bool TryCreateShortcut()
        {
            String shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\CJ Player.lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                return true;
            }
            return false;
        }
        private void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetIconLocation(System.IO.Directory.GetCurrentDirectory() + @"/logo.ico", 1));
            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(APP_ID))
            {
                ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }
        #endregion


        //public void ShowToast(string[] content, string imagePath)
        //{

        //    // Get a toast XML template
        //    XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);// Fill in the text elements
        //    XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
        //    for (int i = 0; i < stringElements.Length; i++)
        //    {
        //        stringElements[i].AppendChild(toastXml.CreateTextNode(content[i]));
        //    }

        //    // Specify the absolute path to an image

        //    XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
        //    imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

        //    // Create the toast and attach event listeners
        //    ToastNotification toast = new ToastNotification(toastXml);
        //    //toast.Activated += ToastActivated;
        //    //toast.Dismissed += ToastDismissed;
        //    //toast.Failed += ToastFailed;

        //    // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
        //    ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        //}

    }
}
