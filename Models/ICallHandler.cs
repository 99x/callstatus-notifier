using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public interface ICallHandler
    {
        void InitializeEvents(Action<string> onCallStarted, Action<string> onCallEnded);
    }
}
