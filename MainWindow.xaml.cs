using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ManagedDependencies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> foundDependencyList;
        private ObservableCollection<string> notFoundDependencyList;
        private List<string> dependantPaths;

        public MainWindow()
        {
            InitializeComponent();
            this.foundDependencyList = new ObservableCollection<string>();
            this.notFoundDependencyList = new ObservableCollection<string>();
            this.ctrlFoundDependencies.ItemsSource = this.foundDependencyList;
            this.ctrlNotFoundDependencies.ItemsSource = this.notFoundDependencyList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                this.ctrlCurrentDll.Text = dlg.FileName;
                this.ProcessDll(dlg.FileName);
            }
        }

        private void ProcessDll(string filename)
        {
            string originalPath = Environment.CurrentDirectory;
            this.foundDependencyList.Clear();
            this.notFoundDependencyList.Clear();

            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(filename);
            string file = System.IO.Path.GetFileName(filename);
            this.AddDll(file, null);
        }

        private void AddDll(string filename, AssemblyName assembly)
        {
            try
            {
                Assembly root = Assembly.ReflectionOnlyLoadFrom(filename);
                AssemblyName[] deps = root.GetReferencedAssemblies();
                this.AddSortedToList(this.foundDependencyList, filename);

                foreach (AssemblyName dep in deps)
                {
                    string file = dep.Name + ".dll";
                    if (!this.foundDependencyList.Contains(file) &&
                        !this.notFoundDependencyList.Contains(file))
                    {
                        this.AddDll(file, dep);
                    }
                }

                return;
            }
            catch (Exception)
            {
            }

            try
            {
                Assembly root = Assembly.ReflectionOnlyLoad(assembly.FullName);
                AssemblyName[] deps = root.GetReferencedAssemblies();
                this.AddSortedToList(this.foundDependencyList, filename);

                foreach (AssemblyName dep in deps)
                {
                    string file = dep.Name + ".dll";
                    if (!this.foundDependencyList.Contains(file) &&
                        !this.notFoundDependencyList.Contains(file))
                    {
                        this.AddDll(file, dep);
                    }
                }

                return;
            }
            catch (Exception)
            {
            }

            this.AddSortedToList(this.notFoundDependencyList, filename);
        }

        private void AddSortedToList(ObservableCollection<string> list, string newEntry)
        {
            int index = 0;
            foreach (string entry in list)
            {
                if (entry.CompareTo(newEntry) > 0)
                {
                    list.Insert(index, newEntry);
                    return;
                }

                index++;
            }

            list.Add(newEntry);
            return;
        }
    }
}
