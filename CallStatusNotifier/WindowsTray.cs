using Models;
using SkypeCallStatusNotifier;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Particle.SDK;
using System.Threading.Tasks;
using System.Collections.Generic;
using Particle.SDK.Models;
using System.Threading;

namespace CallStatusNotifier
{
    internal class WindowsTray
    {
        Container component;
        Icon icon;
        NotifyIcon trayIcon;
        readonly ContextMenuStrip rightClickMenu = new ContextMenuStrip();

        readonly ToolStripMenuItem startNotifier = new ToolStripMenuItem();
        readonly ToolStripMenuItem endNotifier = new ToolStripMenuItem();
        readonly ToolStripMenuItem triggerBuzzer = new ToolStripMenuItem();
        readonly ToolStripMenuItem exit = new ToolStripMenuItem();
        readonly ToolStripSeparator seperator = new ToolStripSeparator();
        readonly ToolStripSeparator seperatorMain = new ToolStripSeparator();
        ParticleDevice myDevice = null;
        bool deviceSuccessfullyInitiated = false;

        public WindowsTray(string[] args)
        {
            InitializeComponent();
            ICallHandler skypeHandler = new SkypeCallHandler();
            if (args.Length > 0)
            {
                Thread.Sleep(90000);
            }
            skypeHandler.InitializeEvents(OnCallStarted, OnCallEnded);
        }

        public void OnCallStarted(string status)
        {
            trayIcon.ShowBalloonTip(200
               , "Call Started"
               , "Please notify others by clicking on this if you are initiate a client call"
               , ToolTipIcon.Info
               );
        }

        public void OnCallEnded(string status)
        {
            trayIcon.ShowBalloonTip(200
                       , "Call Ended"
                       , "Call ended, Please notify other"
                       , ToolTipIcon.Info
                       );
            ChackHostNamesAndEndNotifier();
        }

        private void InitializeComponent()
        {
            icon = new System.Drawing.Icon(@"play-normal-1.ico");
            component = new System.ComponentModel.Container();
            trayIcon = new NotifyIcon(component);
            trayIcon.Text = "Call Notifier";
            trayIcon.Icon = icon;
            trayIcon.Visible = true;
            trayIcon.BalloonTipClicked += new EventHandler(TrayIconBalloonTipClicked);
            trayIcon.ContextMenuStrip = rightClickMenu;

            rightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                startNotifier,
                endNotifier,
                seperatorMain,
                triggerBuzzer,
                seperator,
                exit
            });

            startNotifier.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            startNotifier.Text = "Start Notifier";
            startNotifier.Click += new EventHandler(StartNotifierClicked);

            endNotifier.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            endNotifier.Text = "End Notifier";
            endNotifier.Click += new EventHandler(EndNotifierClicked);

            triggerBuzzer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            triggerBuzzer.Text = "Trigger Buzzer";
            triggerBuzzer.Click += triggerBuzzer_Click;

            exit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            exit.Text = "Exit";
            exit.Click += new EventHandler(ExitClicked);
        }

        void triggerBuzzer_Click(object sender, EventArgs e)
        {
            TriggerBuzzer();
        }

        void ExitClicked(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        void EndNotifierClicked(object sender, EventArgs e)
        {
            ChackHostNamesAndEndNotifier();
        }

        void StartNotifierClicked(object sender, EventArgs e)
        {
            StartPhotonAndNotify();
        }

        public async Task StartPhotonAndNotify()
        {
            await LoginToCloudAndGetDevice();
            if (deviceSuccessfullyInitiated)
            {
                string myFunction = myDevice.Functions[0];
                ParticleFunctionResponse functionResponse = await myDevice.RunFunctionAsync(myFunction, string.Format("on,{0}", Environment.UserName));
            }
        }

        public async Task EndNotifier(bool clearHosts = false)
        {
            string myFunction = myDevice.Functions[0];
            ParticleFunctionResponse functionResponse = await myDevice.RunFunctionAsync(myFunction, string.Format("off,{0}", Environment.UserName));
            if (clearHosts)
            {

                string clearHostFunction = myDevice.Functions[1];
                await myDevice.RunFunctionAsync(clearHostFunction, string.Empty);
            }
            ParticleCloud.SharedCloud.Logout();
        }

        private async Task LoginToCloudAndGetDevice()
        {
            ParticleCloud.SharedCloud.SynchronizationContext = System.Threading.SynchronizationContext.Current;
            var success = await ParticleCloud.SharedCloud.TokenLoginAsync("c078178825f98c72c7ea62fd3eb0cc81207553db");
            if (success)
            {
                List<ParticleDevice> devices = await ParticleCloud.SharedCloud.GetDevicesAsync();
                myDevice = devices[0];

                deviceSuccessfullyInitiated = myDevice.State == ParticleDeviceState.Online;
            }

            if (!deviceSuccessfullyInitiated)
                MessageBox.Show("Device seems offline, please check your internet connectivity and try again.", "Please try again", MessageBoxButtons.OK);
        }

        private async Task ChackHostNamesAndEndNotifier()
        {
            if (myDevice == null)
            {
                await LoginToCloudAndGetDevice();
            }

            if (deviceSuccessfullyInitiated)
            {
                ParticleVariableResponse variableResponse = await myDevice.GetVariableAsync("hostNames");
                if (variableResponse != null)
                {
                    string hostNames = variableResponse.Result;
                    string[] hostNamesList = hostNames.Split(',').Where(s => !string.IsNullOrWhiteSpace(s) && !s.Equals(Environment.UserName)).ToArray();
                    if (hostNamesList.Length > 0)
                    {
                        string message = string.Format("Please note that following nodes has also trigged the notifier,{0}{1}{2}Are you sure you want to turn this off?", Environment.NewLine, string.Join(",", hostNamesList), Environment.NewLine);
                        DialogResult dialogResult = MessageBox.Show(message, "Are you sure you want to turn this off", MessageBoxButtons.YesNo);

                        if (dialogResult == DialogResult.Yes)
                        {
                            await EndNotifier(true);
                        }
                        else
                        {
                            string funcRemoveCurrentUser = myDevice.Functions[2];
                            await myDevice.RunFunctionAsync(funcRemoveCurrentUser, Environment.UserName);
                        }
                    }
                    else
                    {
                        await EndNotifier();
                    }
                }
                else
                {
                    await EndNotifier();
                }
            }
        }

        private async Task TriggerBuzzer()
        {
            if (myDevice == null)
            {
                await LoginToCloudAndGetDevice();
            }

            if (deviceSuccessfullyInitiated)
            {
                string funcTriggerBuzzer = myDevice.Functions[3];
                await myDevice.RunFunctionAsync(funcTriggerBuzzer, string.Empty);
            }
        }

        void TrayIconBalloonTipClicked(object sender, EventArgs e)
        {
            StartPhotonAndNotify();
        }
    }
}
