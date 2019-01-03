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
using System.Windows.Shapes;

namespace FlihtMesseger.Windows
{
    /// <summary>
    /// Interaction logic for MessagesBox.xaml
    /// </summary>
    public partial class MessagesBox : Window
    {
        public MessagesBox()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(main.Text, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(-1);
        }
    }
}
