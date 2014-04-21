namespace WindowsFormsApplication1
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        const int PBST_NORMAL = 0x0001; //Green progressbar, default
        const int PBST_ERROR = 0x0002; //Red progressbar
        const int PBST_PAUSED = 0x0003; //Yellow progressbar
        //const int PBST_PARTIAL = 0x0001; //The blue progressbar is found to have "partial" state - aerostyle.xml

        //*Blue progressbar is not available, since it is not easily available as a progressbar state
        const int PBS_SMOOTHREVERSE = 0x10; //Allows for two-way smooth transition
        const int WM_USER = 0x0400;
        const int PBM_SETSTATE = WM_USER + 16;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public MainForm()
        {
            InitializeComponent();
            //MouseHook.HookMouse();

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 60;
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            progressBar2.Value = 60;
            progressBar3.Minimum = 0;
            progressBar3.Maximum = 100;
            progressBar3.Value = 100;

            SendMessage(progressBar1.Handle, PBM_SETSTATE, (IntPtr)PBST_PAUSED, IntPtr.Zero);
        }
    }
}
