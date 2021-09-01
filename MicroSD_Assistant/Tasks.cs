using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace MicroSD_Assistant
{
    class Tasks
    {
        public void Format(Device device)
        {
            device._state = Device.States.Formating;

            if (FormatDrive_CommandLine(device._driveName[0], "", "FAT32", true, false) == 0)
            {
                device._state = Device.States.Format_Done;
            }
            else
                device._state = Device.States.Error;
        }

        public void StartCopy(Device device, Dictionary<string, string> fileList)
        {
            if (fileList.Count() < 1)
            {
                return;
            }

            device._state = Device.States.Copying;

            int step = 90 / fileList.Count();
            if (step == 0) step = 1;

            foreach (KeyValuePair<string, string> file in fileList)
            {
                if (!File.Exists(device._driveName + "\\" + file.Key))
                    File.Copy(file.Value, device._driveName + "\\" + file.Key);

                device._progress += step;
            }

            device._progress = 100;
            device._state = Device.States.Done;
        }

        public static int FormatDrive_CommandLine(char driveLetter, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int? clusterSize = null)
        {
            #region args check

            if (!Char.IsLetter(driveLetter))
            {
                return -1;
            }

            #endregion
            int err = -1;
            string drive = driveLetter + ":";

            try
            {
                var di = new DriveInfo(drive);
                var psi = new ProcessStartInfo();
                psi.FileName = "format.com";
                psi.CreateNoWindow = true; //if you want to hide the window
                psi.WorkingDirectory = Environment.SystemDirectory;
                psi.Arguments = "/FS:" + fileSystem +
                                             " /Y" +
                                             " /V:" + label +
                                             (quickFormat ? " /Q" : "") +
                                             ((fileSystem == "NTFS" && enableCompression) ? " /C" : "") +
                                             (clusterSize.HasValue ? " /A:" + clusterSize.Value : "") +
                                             " " + drive;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = true;
                var formatProcess = Process.Start(psi);
                var swStandardInput = formatProcess.StandardInput;
                swStandardInput.WriteLine();
                formatProcess.WaitForExit();
                err = formatProcess.ExitCode;
            }
            catch (Exception)
            {

            }

            return err;
        }
    }
}
