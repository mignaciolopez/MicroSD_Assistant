using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroSD_Assistant
{
    class Device
    {
        public enum States
        {
            Error = -1,
            Iddle = 0,
            Formating,
            Format_Done,
            Copying,
            Done
        }

        /********Methods********/
        //public
        public bool Init(int id, string driveName)
        {
            if (id == -1)
                return false;

            _id = id;
            _driveName = driveName;

            _state = States.Iddle;
            _progress = new int();
            _progress = 0;

            _lblDone.Visible = true;
            _lblDone.Text = "Iddle";
            _lblDone.ForeColor = Color.Black;

            _bar.Value = (int)_progress;
            _bar.Visible = true;

            _box.Text = driveName;
            _box.Visible = true;

            return true;
        }

        public void Update()
        {
            UpdateLabel();
            UpdateBar();
            CopyFiles();
        }

        private void UpdateLabel()
        {
            switch (_state)
            {
                case States.Iddle:
                    _lblDone.Text = "Iddle";
                    _lblDone.ForeColor = Color.Black;
                    break;
                case States.Formating:
                    _lblDone.Visible = true;
                    _lblDone.Text = "Formating...";
                    break;
                case States.Copying:
                    _lblDone.Text = "Copying Files...";
                    break;
                case States.Done:
                    _lblDone.Text = "Done!";
                    _lblDone.ForeColor = Color.Green;
                    break;
                default:
                    _lblDone.Text = "Error!";
                    _lblDone.ForeColor = Color.Red;
                    break;
            }
        }

        private void UpdateBar()
        {
            switch (_state)
            {
                case States.Iddle:
                    _bar.Value = 0;
                    break;
                case States.Formating:
                    _bar.Value = 5;
                    break;
                case States.Copying:
                    _bar.Value = _progress;
                    break;
                case States.Done:
                    _bar.Value = 100;
                    break;
                default:
                    _bar.Value = 0;
                    break;
            }
        }

        private void CopyFiles()
        {
            if(_state == States.Format_Done)
            {

            }
        }

        public void Disconected()
        {
            _box.Visible = false;
        }

        //private


        /********Vars********/
        //public
        public int _id;
        public string _driveName;
        public Label _lblDone;
        public GroupBox _box;
        public ProgressBar _bar;
        public States _state;
        public int _progress;

        //private

    }
}
