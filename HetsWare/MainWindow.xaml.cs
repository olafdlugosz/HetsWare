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

        public MainWindow() {
            InitializeComponent();
            InitializeBackgroundWorker();
            //TODO Implement cancelation...
           // backgroundWorker1.WorkerSupportsCancellation = true;
            
        }
        /// <summary>
        /// Method for adding event handlers to the background worker...
        /// </summary>
        private void InitializeBackgroundWorker() {
            this.backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(
            backgroundWorker1_ProgressChanged);

        }
        /// <summary>
        /// Event handler for reporting progress...
        /// </summary>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
                               
            resultLabel.Content = ("Numbers of e-mails per iteration: " + e.ProgressPercentage.ToString());
            TotalDisplayLabel.Content = ("Total number of e-mails sent: " + e.UserState.ToString());
        }

            /// <summary>
            /// Main method for backgroundworkers asynchronous work. It handles the transit of information between threads.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            
            BackgroundWorker worker = sender as BackgroundWorker;
            List<object> asyncThreadPropertyList = e.Argument as List<object>; //<---Properites sent from the main thread via DoWorkEventArgs e.
            string sourcemail = asyncThreadPropertyList[0].ToString();
            string password = asyncThreadPropertyList[1].ToString();
            string targetmail = asyncThreadPropertyList[2].ToString();
            string mailtitle = asyncThreadPropertyList[3].ToString();
            string mailbody = asyncThreadPropertyList[4].ToString();
            bool defaultRecursion = Convert.ToBoolean(asyncThreadPropertyList[5]); // <-- Radiobutton Boolean values.
            bool fibonacci = Convert.ToBoolean(asyncThreadPropertyList[6]);
            bool linear = Convert.ToBoolean(asyncThreadPropertyList[7]);
            bool nuke = Convert.ToBoolean(asyncThreadPropertyList[8]);

            

            if (worker.CancellationPending == true) {
                e.Cancel = true;              
            }
            else {
                if (defaultRecursion == true) {
                    DefaultRecursion(1, 0, sourcemail, password, targetmail, mailtitle, mailbody, worker, e);
                }
                if(fibonacci == true) {
                    Fibonacci(1, 1, 0, 1, 12, sourcemail, password, targetmail, mailtitle, mailbody, worker, e);
                }
                if(linear == true) {
                    Linear(1,0, sourcemail, password, targetmail, mailtitle, mailbody, worker, e);
                }
                if(nuke == true) {
                    Nuke(sourcemail, password, targetmail, mailtitle, mailbody, worker, e);
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
        private void DeployHetsWare(string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody) {
            var client = new SmtpClient("smtp.gmail.com", 587) {
                Credentials = new NetworkCredential(SourceMail, Password),
                EnableSsl = true               
            };
            try {
                client.Send(SourceMail, TargetMail, MailTitle, MailBody);
            } catch (Exception) {

                MessageBoxResult result = MessageBox.Show("Error...Have you enabled: Allow less secure apps in your gmail!?", "Confirmation");
            }           
        }
        //
        //HetsMode Methods:
        //
        private int DefaultRecursion(int n,int total, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody, BackgroundWorker worker1, DoWorkEventArgs e) {
            if (n == 0 && n == 150) //<---stops sending when 150 e-mails are reached.
                return 1;
            
            for (int i = 0; i < n; i++) {
                total += n;
                worker1.ReportProgress(n, total); //<-- sends number of e-mails per iteration + total to the view through an event handler.
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromMinutes(3); //<---If you change this, don't go under 1.5 seconds... Gmail allows only 60 e-mails per minute.
                Thread.Sleep(interval);
            }
            
            TimeSpan timeout = TimeSpan.FromDays(1);   //change timeout if you want to...
            Thread.Sleep(timeout);
            return DefaultRecursion(n + 1,total, SourceMail, Password, TargetMail, MailTitle, MailBody,  worker1, e);
        }
        private void Fibonacci(int a, int b,int total, int counter, int maxNumber, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody, BackgroundWorker worker1, DoWorkEventArgs e) {
            //Use 1, 1, 1, 12 when invoking because 12th call = 144 e-mails.
            for (int i = 0; i < a; i++) {
                total += a;
                worker1.ReportProgress(a, total);  //<-- sends number of e-mails per iteration + total to the view through an event handler.
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromMinutes(3);  //<--If you change this, don't go under 1.5 seconds.. Gmail allows only 60 e-mails per minute.
                Thread.Sleep(interval);                
            }
            TimeSpan timeout = TimeSpan.FromDays(1);  //change timeout if you want to...
            Thread.Sleep(timeout);
            if (counter < maxNumber) Fibonacci(b, a + b, total, counter + 1, maxNumber, SourceMail, Password, TargetMail, MailTitle, MailBody, worker1, e);
        }
        private int Linear(int n, int total, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody, BackgroundWorker worker1, DoWorkEventArgs e) {
            
            if (n == 0 && n == 150)//<---stops sending when 150 e-mails are reached.
                return 1;
            for (int i = 0; i < n; i++) {
                total += n;
                worker1.ReportProgress(n, total); //<-- sends number of e-mails per iteration + total to the view through an event handler.
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromMinutes(3); //<--- Prevents going over the 60 per minute limit. If you change this, don't go under 1.5seconds.
                Thread.Sleep(interval);
            }
            TimeSpan timeout = TimeSpan.FromDays(1); //change timeout if you want to...
            Thread.Sleep(timeout);
            return Linear(n + n, total, SourceMail, Password, TargetMail, MailTitle, MailBody, worker1, e);
        }
        private void Nuke(string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody, BackgroundWorker worker1, DoWorkEventArgs e) {
            int counter = 0;
            for (int i = 0; i < 150; i++) {
                counter++;                
                worker1.ReportProgress(i+1, counter);  //<-- sends number of e-mails per iteration + total to the view through an event handler.              
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromSeconds(2); //<--- 60 mails per minute, remember?
                Thread.Sleep(interval);
            }
        }
            private void DeployButton_Click(object sender, RoutedEventArgs e) {

            List<object>arguments = new List<object>(); //<---list of arguments to migrate to the asynchronous thread.
            arguments.Add(SourceMail);
            arguments.Add(Password);
            arguments.Add(TargetMail);
            arguments.Add(MailTitle);
            arguments.Add(MailBody);
            arguments.Add(DefaultRadioButton.IsChecked);
            arguments.Add(FibonacciRadioButton.IsChecked);
            arguments.Add(LinearRadioButton.IsChecked);
            arguments.Add(NukeRadioButton.IsChecked);
            try {                    
                if (backgroundWorker1.IsBusy != true) {
                    // Start the asynchronous operation.
                    backgroundWorker1.RunWorkerAsync(arguments); //<--- Injecting properties to the asyc thread.
                    // Disable the Start button until 
                    // the asynchronous operation is done.
                    this.DeployButton.IsEnabled = false;
                    this.DeployButton.Content = "HetsWare Deployed...";
                }
            } catch (Exception) {
                    
                    MessageBoxResult result = MessageBox.Show("You have to fill in the form", "Confirmation");
                }            
        }
    }
}
