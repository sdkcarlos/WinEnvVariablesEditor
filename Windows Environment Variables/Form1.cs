using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using Microsoft.Win32;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace Windows_Environment_Variables
{
    public partial class winEnvEditor : Form
    {
        // Import the kernel32 dll.
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        // The declaration is similar to the SDK function
        public static extern bool SetEnvironmentVariable(string lpName, string lpValue);



        const int HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001a;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg,
            UIntPtr wParam, string lParam);


        public winEnvEditor()
        {
            InitializeComponent();
        }

        public static void NotifyUserEnvironmentVariableChanged()
        {
            const int HWND_BROADCAST = 0xffff;
            const uint WM_SETTINGCHANGE = 0x001a;
            SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ListAllEnvironmentVariables();
            setAppColorFromWindows();

            if (!IsUserAdministrator()) 
            {
                MessageBox.Show("You need to start this application in administrador mode in order to edit environment variables. Try Again", "Admin Rights Required",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
            }
        }

        private void ListAllEnvironmentVariables() 
        {
            listView1.Items.Clear();

            int count = 1;

            foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            {
                string[] row1 = { e.Key.ToString(), e.Value.ToString() };
                listView1.Items.Add(count.ToString()).SubItems.AddRange(row1);
                count++;
            }

            var compName = System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string id = listView1.SelectedItems[0].SubItems[0].Text;
                string name = listView1.SelectedItems[0].SubItems[1].Text;
                string value = listView1.SelectedItems[0].SubItems[2].Text;

                editName.Text = name;
                editValue.Text = value;
            }
        }

        public void setAppColorFromWindows()
        {
            var color = System.Drawing.Color.Purple;

            try
            {
                int argbColor = (int)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
                color = System.Drawing.Color.FromArgb(argbColor);
            }
            catch (ArgumentException){}
            catch (Exception) { }

            newSubmit.FlatAppearance.MouseDownBackColor = color;
            newSubmit.FlatAppearance.BorderColor = color;
            editSubmit.FlatAppearance.MouseDownBackColor = color;
            editSubmit.FlatAppearance.BorderColor = color;
            panel2.BackColor = color;
        }

        private void editSubmit_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(editName.Text))
            {
                MessageBox.Show("You need to select a existing variable environment first !","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Do you really want to overwrite this environment variable?  You can't do undo this change, Dont touch anything if you don't know what you're doing !", "Overwrite environment variable",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                setEnvironmentVariable(editName.Text, editValue.Text);
                MessageBox.Show("The environment variable was successfully updated. Restart the application or a new command prompt to see it.", "Environment variable " + editName.Text + " successfully updated",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            }
        }

        private void newSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newName.Text))
            {
                MessageBox.Show("You need to give a name to your variable environment first !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(newValue.Text))
            {
                MessageBox.Show("You need to give a value to your variable environment first !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            setEnvironmentVariable(newName.Text, newValue.Text);

            MessageBox.Show("The environment variable was successfull saved. Restart the application or a new command prompt to see it.", "Environment variable " + newName.Text + " successfully saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);

        }
        
        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }

            return isAdmin;
        }

        public void setEnvironmentVariable(string name,string value) 
        {
            if (IsUserAdministrator())
            {
                using (var envKey = Registry.LocalMachine.OpenSubKey(
                      @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment",
                      true))
                {
                    Contract.Assert(envKey != null, @"registry key is missing!");
                    envKey.SetValue(name, value);
                    SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE,
                        (UIntPtr)0, "Environment");
                }
            }
            else
            {
                MessageBox.Show("You need to start this application in administrador mode in order to execute this action. Try Again", "Admin Rights Required",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(editName.Text))
            {
                MessageBox.Show("You need to select a variable environment first !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Do you really want to DELETE '"+editName.Text+"' environment variable? You can't do undo this change, Dont touch anything if you don't know what you're doing !", "DELETE environment variable",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                setEnvironmentVariable(editName.Text, "");
                MessageBox.Show("The environment variable was successfully deleted. Restart the application or a new command prompt to see it.", "Environment variable " + editName.Text + " successfully deleted",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                newValue.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Choose a file";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string direccion = openFileDialog1.FileName;
                newValue.Text = direccion;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/sdkcarlos/WinEnvVariablesEditor");
        }
 
    }
}
