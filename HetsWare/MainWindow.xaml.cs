using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
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
using System.IO;
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
        }
        /// <summary>
        /// Method for adding event handlers to the background worker...
        /// </summary>
        private void InitializeBackgroundWorker() {
            this.backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(BackgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(
            BackgroundWorker1_ProgressChanged);
        }
        /// <summary>
        /// Event handler for reporting progress...
        /// </summary>
        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {

            //Event variable name is misleading. e.ProgressPercentage is overriden with N of mails per iteration
            resultLabel.Content = ("Number of e-mails per iteration: " + e.ProgressPercentage.ToString()); 
            TotalDisplayLabel.Content = ("Total number of e-mails sent: " + e.UserState.ToString());
            double totalNumber = Convert.ToDouble(e.UserState);
            this.progressBar.Value = totalNumber;
            double totalPercentage = CalculatePercentageForProgressBar(totalNumber);
            ProgressLabel.Content = ($"Tracking progress: {totalPercentage.ToString("#.#")} %");
        }
        private double CalculatePercentageForProgressBar(double totalnumber) {
            double hundredpercent = 100;
            double max = 150;  //<---You will need to change this if change the maximum number of e-mails in the HetsMode methods.
            double onepercent = hundredpercent / max;
            return onepercent * totalnumber;            
        }
        /// <summary>
        /// Main method for backgroundworkers asynchronous work. It handles the transit of information between threads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {

            BackgroundWorker worker = sender as BackgroundWorker;
            List<object> asyncThreadPropertyList = e.Argument as List<object>; //<---Properites sent from the main thread via DoWorkEventArgs e.
            //Properties from Textboxes.
            string sourcemail = asyncThreadPropertyList[0].ToString();
            string password = asyncThreadPropertyList[1].ToString();
            string targetmail = asyncThreadPropertyList[2].ToString();
            string mailtitle = asyncThreadPropertyList[3].ToString();
            string mailbody = asyncThreadPropertyList[4].ToString();
            //Radiobutton Boolean values.
            bool defaultRecursion = Convert.ToBoolean(asyncThreadPropertyList[5]);
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
                if (fibonacci == true) {
                    Fibonacci(1, 1, 0, 1, 12, sourcemail, password, targetmail, mailtitle, mailbody, worker, e);
                }
                if (linear == true) {
                    Linear(1, 0, sourcemail, password, targetmail, mailtitle, mailbody, worker, e);
                }
                if (nuke == true) {
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

                MessageBoxResult result = MessageBox.Show("Error...Have you enabled: Allow less secure apps in your gmail!?..." +
                    "Is is everything in the form correct?", "Confirmation");
            }
        }
        //
        //HetsMode Methods:
        //
        /// <summary>
        /// The Default mode, that will increment the e-mails sent daily by 1.
        /// The method never stops but caps the number of e-mails at 150.
        /// </summary>
        private int DefaultRecursion(int n, int total, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody,
            BackgroundWorker worker1, DoWorkEventArgs e) {

            if (n == 0) { return 1; }; // <--Prevents potential StackOverflow if Zero is accidently used during invocation.
            if (n >= 150) { n = 150; }; //<---Caps the call at 150 e-mails.
            if (worker1.CancellationPending == true) { //<--Cancels the operation.
                e.Cancel = true;
                return 1;
            }
            for (int i = 0; i < n; i++) {
                if (worker1.CancellationPending == true) { //<--Cancels the operation.
                    e.Cancel = true;
                    break;
                }
                total += n;
                worker1.ReportProgress(n, total); //<-- sends number of e-mails per iteration + total to the view through an event handler.
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromSeconds(3); //<---If you change this, don't go under 1.5 seconds... Gmail allows only 60 e-mails per minute.
                Thread.Sleep(interval);                  //Be aware that this affects the restart of call = can't press deploy again untill interval is done.
            }
            if (worker1.CancellationPending == true) { //<--Cancels the operation.
                e.Cancel = true;
                return 1;
            }
            lock (backgroundWorker1) {

                if (worker1.CancellationPending == true) {
                    e.Cancel = true;
                    return 1;
                }
                Monitor.Wait(worker1, TimeSpan.FromDays(1));//<---Change TimeSpan if you want to
                if (worker1.CancellationPending == true) {
                    e.Cancel = true;
                    return 1;
                }
            }
            if (worker1.CancellationPending != true) return DefaultRecursion(n + 1, total, SourceMail, Password, TargetMail, MailTitle, MailBody, worker1, e);
            else return 1;
        }
        /// <summary>
        /// The Fibonacci sequence progression that will work for 12 days then stop.
        /// </summary>
        private int Fibonacci(int a, int b, int total, int counter, int maxNumber, string SourceMail, string Password, string TargetMail,
            string MailTitle, string MailBody, BackgroundWorker worker1, DoWorkEventArgs e) {
            //Use 1, 1, 1, 12 when invoking because 12th call = 144 e-mails.
            if (worker1.CancellationPending == true) { //<--Cancels the operation.
                e.Cancel = true;
                return 1;
            }
            for (int i = 0; i < a; i++) {
                if (worker1.CancellationPending == true) { //<--Cancels the operation.
                    e.Cancel = true;
                    break;
                }
                total += a;
                worker1.ReportProgress(a, total);  //<-- sends number of e-mails per iteration + total to the view through an event handler.
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromSeconds(2);  //<--If you change this, don't go under 1.5 seconds.. Gmail allows only 60 e-mails per minute.
                Thread.Sleep(interval);
            }
            if (worker1.CancellationPending == true) { //<--Cancels the operation.
                e.Cancel = true;
                return 1;
            }
            lock (backgroundWorker1) {

                if (worker1.CancellationPending == true) {
                    e.Cancel = true;
                }
                Monitor.Wait(worker1, TimeSpan.FromDays(1));//<---Change TimeSpan if you want to
                if (worker1.CancellationPending == true) {
                    e.Cancel = true;
                }
            }
            if (counter < maxNumber) return Fibonacci(b, a + b, total, counter + 1, maxNumber, SourceMail, Password, TargetMail, MailTitle, MailBody, worker1, e);
            else return 1;
        }
        /// <summary>
        /// The Linear polynomial progression that will increment the number of e-mails daily count by the sum of itself.
        /// The method never stops working but it caps the number of e-mails sent daily at 150.
        /// </summary>
        private int Linear(int n, int total, string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody,
            BackgroundWorker worker1, DoWorkEventArgs e) {
            if (worker1.CancellationPending == true) { //<--Cancels the operation.
                e.Cancel = true;
                return 1;
            }
            if (n == 0) { return 1; }; // <--Prevents potential StackOverflow if Zero is accidently used during invocation.
            if (n >= 150) { n = 150; }; //<---Caps the call at 150 e-mails.
            for (int i = 0; i < n; i++) {
                if (worker1.CancellationPending == true) { //<--Cancels the operation.
                    e.Cancel = true;
                    break;
                }
                total += n;
                worker1.ReportProgress(n, total); //<-- sends number of e-mails per iteration + total to the view through an event handler.
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromSeconds(2); //<--- Prevents going over the 60 per minute limit. If you change this, don't go under 1.5seconds.
                Thread.Sleep(interval);
            }
            if (worker1.CancellationPending == true) { //<--Cancels the operation.
                e.Cancel = true;
                return 1;
            }
            lock (backgroundWorker1) {

                if (worker1.CancellationPending == true) {
                    e.Cancel = true;
                }
                Monitor.Wait(worker1, TimeSpan.FromDays(1));//<---Change TimeSpan if you want to
                if (worker1.CancellationPending == true) {
                    e.Cancel = true;
                }
            }
            return Linear(n + n, total, SourceMail, Password, TargetMail, MailTitle, MailBody, worker1, e);
        }
        /// <summary>
        /// The Nuke mode that will send 150 e-mails as quickly as gmail allows without blocking the user for a couple hours.
        /// This method stops after 150 e-mails.
        /// </summary>
        private void Nuke(string SourceMail, string Password, string TargetMail, string MailTitle, string MailBody,
            BackgroundWorker worker1, DoWorkEventArgs e) {
            int total = 0;
            for (int i = 0; i < 150; i++) {
                if (worker1.CancellationPending == true) { //<--Cancels the operation.
                    e.Cancel = true;
                    break;
                }
                total++;
                worker1.ReportProgress(1, total);  //<-- sends number of e-mails per iteration + total to the view through an event handler.              
                DeployHetsWare(SourceMail, Password, TargetMail, MailTitle, MailBody);
                TimeSpan interval = TimeSpan.FromSeconds(2); //<--- 60 mails per minute, remember?
                Thread.Sleep(interval);
            }
        }
        private void WakeUpMonitor() {
            lock (backgroundWorker1) {
                Monitor.Pulse(backgroundWorker1);
            }
        }
        private void PlaySound() {
            string filename = @"HetsWareDeployedSound.wav";
            string path = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\Sounds\", filename);

            using (var soundPlayer = new SoundPlayer(path)) {
                soundPlayer.Play();
            }
        }
        private void DeployButton_Click(object sender, RoutedEventArgs e) {

            List<object> arguments = new List<object>(){ //<---list of arguments to migrate to the asynchronous thread.
                //Textbox Properties
                SourceMail,
                Password,
                TargetMail,
                MailTitle,
                MailBody,
                //Radiobutton boolean values
                DefaultRadioButton.IsChecked,
                FibonacciRadioButton.IsChecked,
                LinearRadioButton.IsChecked,
                NukeRadioButton.IsChecked };
            try {
                //Wake up the async thread in case it was sleeping. (I use monitor instead of ThreadSleep.
                //Otherwise starting over with long timeouts, didn't correctly.)
                WakeUpMonitor();
                if (backgroundWorker1.IsBusy != true) {
                    // Start the asynchronous operation.
                    backgroundWorker1.RunWorkerAsync(arguments); //<--- Injecting properties to the asyc thread.
                    // Disable the Start button until 
                    // the asynchronous operation is done.
                    this.DeployButton.IsEnabled = false;
                    this.DeployButton.Content = "HetsWare Deployed...";
                    // Enable the Cancel button while 
                    // the asynchronous operation runs.
                    this.CancelButton.IsEnabled = true;
                    //Play the "HetsWare Deployed" Sound
                    PlaySound();

                }
            } catch (Exception) {

                MessageBoxResult result = MessageBox.Show("You have to fill in the form", "Confirmation");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            // Cancel the asynchronous operation.
            
            this.backgroundWorker1.CancelAsync();
            //Enable the DeployButton again
            DeployButton.IsEnabled = true;
            this.DeployButton.Content = "Börja Hetsa!";
            // Disable the Cancel button.
            CancelButton.IsEnabled = false;
            //Reset the label content
            resultLabel.Content = ("Number of e-mails per iteration: ");
            TotalDisplayLabel.Content = ("Total number of e-mails sent: ");
            //Reset the progressBar
            this.progressBar.Value = 0;
            //Reset the progress label
            ProgressLabel.Content = ("Tracking progress: ");

        }
    }
}
