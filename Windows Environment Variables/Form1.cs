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

namespace Windows_Environment_Variables
{
    public partial class Form1 : Form
    {
        // Import the kernel32 dll.
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        // The declaration is similar to the SDK function
        public static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ListAllEnvironmentVariables();

            setAppColorFromWindows();

            
        }

        private void ListAllEnvironmentVariables() 
        {
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
        /// method for setting the variable of an environment variable associated with
        /// the current running process
        /// </summary>
        /// <param name="variable">variable to set</param>
        /// <param name="value">value to set the variable to</param>
        /// <returns></returns>
        public static bool SetVariable(string variable, string value)
        {
            try
            {
                // Get the write permission to set the environment variable.
                EnvironmentPermission permissions = new EnvironmentPermission(EnvironmentPermissionAccess.Write, variable);

                permissions.Demand();

                return SetEnvironmentVariable(variable, value);
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(string.Format("Error while setting variable {0} : {1}", variable, ex.Message));
            }
            return false;
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


            newSubmit.FlatAppearance.MouseDownBackColor = color;
            newSubmit.FlatAppearance.BorderColor = color;
            editSubmit.FlatAppearance.MouseDownBackColor = color;
            editSubmit.FlatAppearance.BorderColor = color;
            panel2.BackColor = color;
        }

        private void editSubmit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to overwrite this environment variable? You can't do undo this change.", "Overwrite environment variable",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                //TODO: Stuff
            }
        }
 
    }
}
