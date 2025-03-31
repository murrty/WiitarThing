using System;
using System.Windows;

namespace WiitarThing
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        private ErrorWindow()
        {
            InitializeComponent();
        }

        public ErrorWindow(Exception ex)
            : this()
        {
            string msg = ex.Message;
            string stack = ex.StackTrace;

            if (ex.Message.Contains("NintrollerLib"))
            {
                Version nVersion = System.Reflection.Assembly.LoadFrom("Nintroller.dll").GetName().Version;
                if (nVersion < new Version(2, 5))
                {
                    msg = "The Nintroller library is out of date.";
                    stack = "Please try the following:" + Environment.NewLine +
                        Environment.NewLine + "1) Uninstall WiinUSoft" +
                        Environment.NewLine + "2) Reinstall WiinUSoft using the latest installer" +
                        Environment.NewLine + "3) Verify that the installed Nintroller.dll in the installation folder" +
                        " is version 2.5 by right clicking the file, choosing Properties, and choose the Details tab.";
                    _dontSendBtn.Content = "Close";
                }
            }
            else if (ex is System.Windows.Markup.XamlParseException) {
                msg = "WPF XAML parsing failed";
                stack = "The WPF parsing failed, a possible bug causing the parsing due to color profiles may be the cause of the exception." +
                    Environment.NewLine + "A method of fixing involves setting the device color profile to 'sRGB IEC61966-2.1' (see this issue on github for information: https://github.com/TheNathannator/WiitarThing/issues/13)" +
                    Environment.NewLine +
                    Environment.NewLine + ex.StackTrace;
            }

            _errorMessage.Content = msg;
            _errorStack.Text = stack;
        }

        private void _dontSendBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }
    }
}
