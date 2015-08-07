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

namespace Windows_Environment_Variables
{
    public partial class Form1 : Form
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


        public Form1()
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

        

        /// <summary>
        /// method for getting all available environment variables
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetEnvironmentVariables()
        {
            try
            {
                //dictionary object to hold the key/value pairs
                Dictionary<string, string> variables = new Dictionary<string, string>();

                //loop through the list and add to our dictionary list
                Parallel.ForEach<DictionaryEntry>(Environment.GetEnvironmentVariables().OfType<DictionaryEntry>(), entry =>
                {
                    variables.Add(entry.Key.ToString(), entry.Value.ToString());
                });

                return variables;
            }
            catch (SecurityException ex)
            {
                Console.WriteLine("Error retrieving environment variables: {0}", ex.Message);
                return null;
            }
        }


        /// <summary>
        /// method for retrieving an environment variable by it's name
        /// </summary>
        /// <param name="name">the name of the environment variable we're looking for</param>
        /// <returns></returns>
        public string GetEnvironmentVariableByName(string name)
        {
            try
            {
                //get the variable
                string variable = Environment.GetEnvironmentVariable(name);

                //check for a value, if nothing is returned let the application know
                if (!string.IsNullOrEmpty(variable))
                    return variable;
                else
                    return "The requested environment variable could not be found";
            }
            catch (SecurityException ex)
            {
                Console.WriteLine(string.Format("Error searching for environment variable: {0}", ex.Message));
                return string.Empty;
            }
        }

        private bool DoesEnvironmentVariableExist(string variable, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            try
            {
                return string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable, target)) ? false : true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }



        /// <summary>
        /// method for handling environment variable values. Will:
        /// Update
        /// Create
        /// Delete
        /// </summary>
        /// <param name="variable">the variable we're looking for</param>
        /// <param name="value">the value to set it to (null for deleting a variable)</param>
        /// <param name="target">the targer, defaults to User</param>
        /// <returns></returns>
        public void SetEnvironmentVariableValue(string variable, string value = null, EnvironmentVariableTarget target = EnvironmentVariableTarget.User)
        {
            try
            {
                //first make sure a name is provided
                if (string.IsNullOrEmpty(variable))
                    throw new ArgumentException("Variable names cannot be empty or null", "variable");

                if (!DoesEnvironmentVariableExist(variable, target))
                    throw new Exception(string.Format("The environment variable {0} was not found", variable));
                else
                    Environment.SetEnvironmentVariable(variable, value, target);

                Console.WriteLine(string.Format("The environment variable {0} has been deleted", variable));

            }
            catch (SecurityException ex)
            {
                Console.WriteLine(string.Format("Error setting environment variable {0}: {1}", variable, ex.Message));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(string.Format("Error setting environment variable {0}: {1}", variable, ex.Message));
            }
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
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
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

                ListAllEnvironmentVariables();
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

            if (MessageBox.Show("Do you really want to DELETE '"+editName+"' environment variable? You can't do undo this change, Dont touch anything if you don't know what you're doing !", "DELETE environment variable",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                setEnvironmentVariable(editName.Text, "");
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
 
    }
}
