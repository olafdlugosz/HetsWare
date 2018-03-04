using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

namespace HetsWare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SourceMail => SourceMailTextBox.Text;
        private string Password => PasswordTextBox.Password;
        private string TargetMail => TargetMailTextBox.Text;
        private string MailTitle => MailTitleTextBox.Text;
        private string MailBody => MailBodyTextBox.Text;
        private BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private List<object> arguments;

        public MainWindow() {
            backgroundWorker1 = new BackgroundWorker();
            InitializeComponent();
            InitializeBackgroundWorker();
            backgroundWorker1.WorkerSupportsCancellation = true;
            arguments = new List<object>(); //<---list of arguments to migrate to the asynchronous thread.
            arguments.Add(SourceMail);
            arguments.Add(Password);
            arguments.Add(TargetMail);
            arguments.Add(MailTitle);
            arguments.Add(MailBody);
            arguments.Add(DefaultRadioButton.IsChecked);
            arguments.Add(FibonacciRadioButton.IsChecked);
            arguments.Add(LinearRadioButton.IsChecked);
            arguments.Add(NukeRadioButton.IsChecked);
        }
        /// <summary>
        /// Method for adding events handlers to the background worker...
        /// </summary>
        private void InitializeBackgroundWorker() {
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
        }
        /// <summary>
        /// Main method for backgroundworkers asynchronious work. It handles the transit of information between threads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            
            BackgroundWorker worker = sender as BackgroundWorker;
            List<object> genericlist = e.Argument as List<object>; //<---Properites sent from the main thread.
            string sourcemail = genericlist[0].ToString();
            string password = genericlist[1].ToString();
            string targetmail = genericlist[2].ToString();
            string mailtitle = genericlist[3].ToString();
            string mailbody = genericlist[4].ToString();
            bool defaultRecursion = Convert.ToBoolean(genericlist[5]); // <-- Radiobutton Boolean values.
            bool fibonacci = Convert.ToBoolean(genericlist[6]);
            bool linear = Convert.ToBoolean(genericlist[7]);
            bool nuke = Convert.ToBoolean(genericlist[8]);

            if (worker.CancellationPending == true) {
                e.Cancel = true;
            }
            else {
                if (defaultRecursion == true) {
                    DefaultRecursion(1, sourcemail, password, targetmail, mailtitle, mailbody);
                }
                if(fibonacci == true) {
                    Fibonacci(1, 1, 1, 12, sourcemail, password, targetmail, mailtitle, mailbody);
                }
                if(linear == true) {
                    Linear(1, sourcemail, password, targetmail, mailtitle, mailbody);
                }
                if(nuke == true) {
                    Nuke(sourcemail, password, targetmail, mailtitle, mailbody);
                }
            }
        }/// <summary>
        /// Main Method for sending the e-mail.
        /// </summary>
        /// <param name="SourceMail">Sender e-mail</param>
        /// <param name="Password">Sender Password</param>
        /// <param name="TargetMail">Recipient e-mail</param>
        /// <param name="MailTitle">E-mail Title</param>
        /// <param name="MailBody">E-mail body</param>
        static void DeployHetsWare(string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody) {
            var client = new SmtpClient("smtp.gmail.com", 587) {
                Credentials = new NetworkCredential(SourceMail, Password),
                EnableSsl = true               
            };        
                client.Send(SourceMail, TargetMail, MailTitle, MailBody);            
        }
        //
        //HetsMode Methods:
        //
        static int DefaultRecursion(int n, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody) {
            if (n == 0 && n == 150) //<---stops sending when 150 e-mails are reached.
                return 1;
            for (int i = 0; i < n; i++) {
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromMinutes(1.5); //<---Don't change this.. Gmail allows only 60 e-mails per minute.
                Thread.Sleep(interval);
            }                       
            TimeSpan timeout = TimeSpan.FromDays(1);   //change timeout if you want to...
            Thread.Sleep(timeout);
            return DefaultRecursion(n + 1, SourceMail, Password, TargetMail, MailTitle, MailBody);
        }
        static void Fibonacci(int a, int b, int counter, int maxNumber, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody) {
            //Use 1, 1, 1, 12 because 12th call = 144 e-mails.
            for (int i = 0; i < a; i++) {
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromMinutes(2);  //<--Don't change this.. Gmail allows only 60 e-mails per minute.
                Thread.Sleep(interval);
            }
            TimeSpan timeout = TimeSpan.FromDays(1);  //change timeout if you want to...
            Thread.Sleep(timeout);
            if (counter < maxNumber) Fibonacci(b, a + b, counter + 1, maxNumber, SourceMail, Password, TargetMail, MailTitle, MailBody);
        }
        static int Linear(int n, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody) {
            if (n == 0 && n == 150)//<---stops sending when 150 e-mails are reached.
                return 1;
            for (int i = 0; i < n; i++) {
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
            }
            TimeSpan timeout = TimeSpan.FromDays(1);
            Thread.Sleep(timeout);
            return Linear(n + n, SourceMail, Password, TargetMail, MailTitle, MailBody);
        }
        static void Nuke(string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody) {
            for (int i = 0; i < 150; i++) {                
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromMinutes(1.5);
                Thread.Sleep(interval);
            }
        }
            private void DeployButton_Click(object sender, RoutedEventArgs e) {
            try {                    
                if (backgroundWorker1.IsBusy != true) {
                    // Start the asynchronous operation.
                    backgroundWorker1.RunWorkerAsync(arguments);
                    // Disable the Start button until 
                    // the asynchronous operation is done.
                    this.DeployButton.IsEnabled = false;
                    this.DeployButton.Content = "HetsWare Deployed...";
                }
            } catch (Exception) {
                    DisplayTextBlock.Text = " ";
                    MessageBoxResult result = MessageBox.Show("You have to fill in the form", "Confirmation");
                }            
        }
    }
}
