using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SugarCpp.Compiler;
using System.Threading;

namespace SugarCpp.Watcher
{
    public partial class MainWindow : Form
    {
        private Dictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();
        private Dictionary<object, string> watcher_to_root = new Dictionary<object, string>();
        private Dictionary<string, Dictionary<string, string>> compile_info = new Dictionary<string, Dictionary<string, string>>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddTaskButton_Click(object sender, EventArgs e)
        {
            string path = new DirectoryInfo(this.TaskTextBox.Text).FullName;
            if (watchers.ContainsKey(path))
            {
                MessageBox.Show(string.Format("This task already exists!"));
                return;
            }
            FileSystemWatcher watcher = new FileSystemWatcher();
            if (!Directory.Exists(path + "/.sugar"))
            {
                Directory.CreateDirectory(path + "/.sugar");
            }
            watcher.Path = path + "/.sugar";

            watcher.Changed += this.OnWatcherEvent;
            watcher.Renamed += this.OnWatcherEvent;

            watcher.Filter = "*.sc";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            watcher_to_root[watcher] = path;
            compile_info[path] = new Dictionary<string, string>();

            this.TaskList.Items.Add(path);

            this.TaskTextBox.Text = "";
        }

        private void OnWatcherEvent(object sender, FileSystemEventArgs e)
        {
            this.Invoke(new Action(() => OnFileEvent(sender, e)));
        }

        private void OnFileEvent(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(1);

            string name = e.Name.Substring(0, e.Name.Length - 3);
            name = name.Substring(name.LastIndexOf("/") + 1);
            name = name.Substring(name.LastIndexOf("\\") + 1);

            string root = watcher_to_root[sender];
            string input = null;
            try
            {
                input = File.ReadAllText(e.FullPath);
            }
            catch (Exception)
            {
                compile_info[root][name] = string.Format("Failed to read file.");
                UpdateGui();
                return;
            }

            TargetCppResult result = null;
            try
            {
                result = SugarCompiler.Compile(input, name);
            }
            catch (Exception err)
            {
                compile_info[root][name] = string.Format("Compile Error:\n{0}", err);
                Console.WriteLine("Compile Error!");
                UpdateGui();
                return;
            }

            try
            {
                File.WriteAllText(root + "/" + name + ".h", result.Header);
                File.WriteAllText(root + "/" + name + ".cpp", result.Implementation);
            }
            catch (Exception)
            {
                compile_info[root][name] = string.Format("Can't access file.");
                UpdateGui();
                return;
            }

            compile_info[root][name] = null;
            Console.WriteLine("Compile Success!");
            UpdateGui();
        }

        private void UpdateGui()
        {
            if (TaskList.SelectedIndex != -1)
            {
                string file_name = this.FileList.Items.Count == 0 ? null : this.FileList.Items[0].Text;

                string root = (string)TaskList.SelectedItem;
                this.FileList.BeginUpdate();
                this.FileList.Items.Clear();
                foreach (var file in compile_info[root])
                {
                    ListViewItem item = new ListViewItem(file.Key);
                    if (file.Value != null)
                    {
                        item.BackColor = Color.Red;
                    }
                    this.FileList.Items.Add(item);
                }

                ListViewItem selected_item = null;
                foreach (var item in this.FileList.Items)
                {
                    if (((ListViewItem)item).Text == file_name)
                    {
                        selected_item = (ListViewItem)item;
                        break;
                    }
                }

                if (selected_item != null)
                {
                    selected_item.Selected = true;
                }

                this.FileList.EndUpdate();
            }
        }

        private void UpdateCompileInfo()
        {
            if (TaskList.SelectedIndex != -1)
            {
                if (this.FileList.SelectedItems.Count > 0)
                {
                    ListViewItem item = this.FileList.SelectedItems[0];
                    string info = compile_info[(string)TaskList.SelectedItem][item.Text];
                    this.CompileInfo.Text = info;
                }
            }
        }

        private void TaskList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void FileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCompileInfo();
        }
    }
}
