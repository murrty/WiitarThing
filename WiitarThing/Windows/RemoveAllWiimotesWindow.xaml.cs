using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WiitarThing.Windows
{
    /// <summary>
    /// Interaction logic for RemoveAllWiimotesWindow.xaml
    /// </summary>
    public partial class RemoveAllWiimotesWindow : Window
    {
        System.Threading.Tasks.Task _workTask;
        System.Threading.CancellationTokenSource _cancelToken;

        public RemoveAllWiimotesWindow()
        {
            InitializeComponent();
            labelCancelling.Visibility = Visibility.Hidden;
            _cancelToken = new System.Threading.CancellationTokenSource();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buttonCancel.IsEnabled = true;
            _workTask = System.Threading.Tasks.Task.Run(() =>
            {
                SyncWindow.RemoveAllWiimotes(_cancelToken.Token);
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Close()));
            });
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            _cancelToken.Cancel();
            buttonCancel.IsEnabled = false;
            labelCancelling.Visibility = Visibility.Visible;
        }
    }
}
