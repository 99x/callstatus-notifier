using System;
using System.Linq;

namespace CallStatusNotifier
{
    static class Program
    {
        /// 
        /// The main entry point for the application.
        /// 
        [STAThread]
        static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            new WindowsTray(args);
            System.Windows.Forms.Application.Run();
        }
    }
}
