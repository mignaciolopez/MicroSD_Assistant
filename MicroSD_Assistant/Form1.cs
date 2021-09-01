using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Management;

namespace MicroSD_Assistant
{
    public partial class frm_Main : Form
    {
        private Dictionary<string, string> fileList = new Dictionary<string, string>(); //<FileName, Full Path + FileName>
        private Dictionary<int, Device> devices = new Dictionary<int, Device>();
        private Dictionary<int, Tasks> tasks = new Dictionary<int, Tasks>();

        private enum EventType
        {
            Arrival = 2,
            Removal = 3
        }

        public frm_Main()
        {
            InitializeComponent();
            Application.Idle += HandleApplicationIdle;
        }

        bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }

        void HandleApplicationIdle(object sender, EventArgs e)
        {
            while (IsApplicationIdle())
            {
                update();
            }
        }

        void update()
        {
            foreach(KeyValuePair<int, Device> d in devices)
            {
                d.Value.Update();
            }
        }

        private int getID()
        {
            int id = new int();
            id = 0;

            while (devices.ContainsKey(id))
                id++;

            if (id >= 24)
                return -1;

            return id;
        }

        private void OnVolumeChangeEvent(object sender, EventArrivedEventArgs e)
        {
            string driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
            EventType eventType = (EventType)(Convert.ToInt16(e.NewEvent.Properties["EventType"].Value));

            string eventName = Enum.GetName(typeof(EventType), eventType);

            //MessageBox.Show(DateTime.Now.ToString() + " " + driveName + " " + eventName);
            switch (eventType)
            {
                case EventType.Arrival:
                    OnVolumeArrived(driveName);
                    break;
                case EventType.Removal:
                    OnVolumeRemoved(driveName);
                    break;
                default:
                    MessageBox.Show(DateTime.Now.ToString() + "Event not Handled: " + driveName + " " + eventName);
                    break;
            }
        }

        private void OnVolumeArrived(string driveName)
        {
            int id = getID();

            Device device = new Device();

            Control[] controls = this.Controls.Find("lbl_Done" + id.ToString(), true);
            if (controls.Length > 0)
                device._lblDone = controls[0] as Label;

            controls = this.Controls.Find("bar" + id.ToString(), true);
            if (controls.Length > 0)
                device._bar = controls[0] as ProgressBar;

            controls = this.Controls.Find("box" + id.ToString(), false);
            if (controls.Length > 0)
                device._box = controls[0] as GroupBox;

            this.Invoke(new Action(delegate ()
            {
                if (device.Init(id, driveName))
                    devices.Add(id, device);
                else
                    MessageBox.Show(DateTime.Now.ToString() + "Error creating Device: " + driveName);
            }));
        }

        public void OnVolumeRemoved(string driveName)
        {
            foreach (KeyValuePair<int, Device> device in devices)
            {
                if (device.Value._driveName == driveName)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        device.Value.Disconected();
                    }));

                    devices.Remove(device.Key);
                    break;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

        private void folderBrowser(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    this.txt_path.Text = fbd.SelectedPath.ToString();

                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    foreach (string fileName in files)
                        fileList.Add(fileName.Remove(0, fbd.SelectedPath.Length + 1), fileName);

                    //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                }
            }
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            folderBrowser(sender, e);
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3");

            watcher.EventArrived += OnVolumeChangeEvent;

            watcher.Query = query;
            watcher.Start();
        }
    }
}
