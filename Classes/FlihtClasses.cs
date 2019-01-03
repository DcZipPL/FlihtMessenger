using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace FlihtMesseger.Classes
{
    class User
    {
        public string username;
        public string usertoken;
    }

    class Base64
    {
        public static string Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Decode(string plainText)
        {
            var plainTextBytes = Convert.FromBase64String(plainText);
            return Encoding.UTF8.GetString(plainTextBytes);
        }
    }

    class Badge
    {
        public static void SetNumber(int num, TaskbarItemInfo badge)
        {
            if (num > 0 && num < 10)
            {
                badge.Overlay = new BitmapImage(new Uri($"pack://application:,,,/FlihtMesseger;component/Icons/Badges/NewMessage_{num}.png", UriKind.Absolute));
            }
            else
            {
                badge.Overlay = new BitmapImage(new Uri($"pack://application:,,,/FlihtMesseger;component/Icons/Badges/NewMessage_0.png", UriKind.Absolute));
            }
        }

        public static void Clear(TaskbarItemInfo badge)
        {
            badge.Overlay = null;
        }
    }

    class Toaster
    {
        string _APP_ID = "null";
        public Toaster(string APP_ID)
        {
            _APP_ID = APP_ID;
        }

        public void Toast(string message, string username)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = username
                            },
                            new AdaptiveText()
                            {
                                Text = message
                            }
                        },
                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = "Fliht Messenger"
                        }
                    }
                },
                Launch = "action=viewStory&storyId=92187"
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(GetXml(toastContent));

            // And send the notification
            ToastNotificationManager.CreateToastNotifier(_APP_ID).Show(toastNotif);
        }

        private XmlDocument GetXml(ToastContent toastContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(toastContent.GetContent());

            return doc;
        }
    }
}
