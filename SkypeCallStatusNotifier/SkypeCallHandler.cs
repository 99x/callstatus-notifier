using Models;
using SKYPE4COMLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SkypeCallStatusNotifier
{
    public class SkypeCallHandler : ICallHandler
    {
        public void InitializeEvents(Action<string> onCallStarted, Action<string> onCallEnded)
        {
            Skype skype = new Skype();
            _ISkypeEvents_Event events = skype;

            events.CallStatus += (call, status) => OnCallStatusChanged(call, status, onCallStarted, onCallEnded);
            // TODO: Handle unsubscription.
            // https://msdn.microsoft.com/en-us/library/ms366768.aspx
            // events.CallStatus -= (call, status) => OnCallStatusChanged(call, status, onCallStarted, onCallEnded);

            try
            {
                if (skype.Client.IsRunning)
                    skype.Attach();
            }
            catch (Exception)
            {
                // Ignore any exceptions thrown (TODO: These can be logged to observe later, if needed)
            }
        }

        private void OnCallStatusChanged(Call call, TCallStatus status, Action<string> onCallStarted, Action<string> onCallEnded)
        {
            if (status == TCallStatus.clsRinging || status == TCallStatus.clsRouting)
            {
                onCallStarted(status.ToString());
            }
            else if (status == TCallStatus.clsFinished || status == TCallStatus.clsRefused)
            {
                onCallEnded(status.ToString());
            }
        }
    }
}
