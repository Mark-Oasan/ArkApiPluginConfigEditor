using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Diagnostics.Metrics;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace ArkApiPluginConfigEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class TreeNode
    {
        private MainWindow mainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);

        private bool _isSelected;
        private string _name;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    mainWindow.SelectedItem = this;
                    Debug.WriteLine("IsSelected is Called: " + this.Name.ToString());
                    //mainWindow.lb_name.Content = this.Name.ToString();
                    //mainWindow.tb_value.Text = this.Value.ToString();
                    mainWindow.btn_Save.IsEnabled = true;
                    //Debug.WriteLine("Selected triggered! " + this.Value.ToString());
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
            }
        }
    }
    public class TreeObject : TreeNode
    {
        private ObservableCollection<TreeNode> _children;
        public ObservableCollection<TreeNode> Children
        {
            get => _children;
            set
            {
                _children = value;
            }
        }
    }
    public class TreeValue : TreeNode
    {
        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                //Debug.WriteLine("Value was SET");
            }
        }
    }

    public partial class MainWindow : Window
    {
        private TreeNode _selectedItem;
        private ObservableCollection<TreeNode> _treeItems;
        private string pluginBaseDir = "\\ShooterGame\\Binaries\\Win64\\ArkApi\\Plugins";
        private string selectedPath;
        private string selectedPlugin;
        private string combinedPluginDir;
        private bool RawJsonHasChanged = false;
        public ObservableCollection<KeyValuePair<string, string>> Items { get; set; }
        public List<string> Maps { get; set; }
        public List<string> Excluded { get; set; }
        public List<string> Plugins { get; set; }
        public ObservableCollection<TreeNode> TreeItems
        {
            get => _treeItems;
            set
            {
                _treeItems = value;
            }
        }
        public TreeNode SelectedItem
        {
            get
            {
                Debug.WriteLine("SelectedItem GET CALLED: " + _selectedItem.Name.ToString());
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                Debug.WriteLine("SelectedItem SET CALLED: " + value.Name.ToString());
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            Maps = new List<string>();
            Excluded = new List<string>();
            Plugins = new List<string>();
            TreeItems = new ObservableCollection<TreeNode>();
        }
        
        
        private void GetMaps()
        {
            string[] servers = Directory.GetDirectories(selectedPath, "*", SearchOption.TopDirectoryOnly);

            //MessageBox.Show(String.Join(", ", servers));

            Maps.Clear();
            Plugins.Clear();

            // Set Maps collection
            foreach (string dir in servers)
            {
                string serverName = dir.Split("\\").Last();
                Maps.Add(serverName);
            }
            // Set base plugin reference
            combinedPluginDir = selectedPath + "\\" + Maps.First().ToString() + pluginBaseDir;

            lb_Maps.ItemsSource = Maps;
            lb_Maps.Items.Refresh();
            
            EnableDisableControls();
            GetPlugins();
        }
        private void GetPlugins()
        {
            string[] pluginsPath = new string[] { };
            List<string> tmpPlugins = new List<string>();
            // Counter of all plugin (should match to every servers)
            int counter = 0;
            string pluginToBeRemove = null;

            // Get all plugin directory
            try
            {
                pluginsPath = Directory.GetDirectories(combinedPluginDir, "*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                lb_Error.Content = $"Error: {e.Message.ToString()}";
            }


            if (pluginsPath.Count() > 0)
            {
                foreach (string pPath in pluginsPath)
                {
                    string pluginName = pPath.Split("\\").Last();

                    // Check if plugin exists
                    int i = Plugins.FindIndex(a => a.ToString() == pluginName);

                    if (i == -1)
                    {
                        //Debug.WriteLine($"NOT EXIST Map: {serverName} Plugin: {pluginName}");
                        if (File.Exists(combinedPluginDir + "\\" + pluginName + "\\config.json"))
                            Plugins.Add(pluginName);
                    }
                    else
                    {
                        //Debug.WriteLine($"EXIST Map: {serverName} Plugin: {pluginName}");
                        counter++;
                        tmpPlugins.Add(pluginName);
                    }
                }
            }

            if (counter != 0 && counter < Plugins.Count())
            {
                //Debug.WriteLine($"COUNTER IS LESS {counter} {Plugins.Count()}");
                foreach (string plugins in Plugins)
                {
                    int j = tmpPlugins.FindIndex(a => a.ToString() == plugins);
                    if (j < 0)
                    {
                        //Debug.WriteLine($"Map: {serverName} Missing Plugin: {plugins}");
                        lb_Error.Content = $"Map: servername Missing Plugin: {plugins} excluded to the list.";
                        pluginToBeRemove = plugins;
                    }
                }
                counter = 0;
            }
            else
            {
                //Debug.WriteLine($"COUNTER IS IDK {counter} {Plugins.Count()}");
                tmpPlugins.Clear();
                counter = 0;
            }

            if (pluginToBeRemove != null)
            {
                Plugins.Remove(pluginToBeRemove);
            }

            cmb_Plugins.ItemsSource = Plugins;
            cmb_Plugins.Items.Refresh();
            if (Plugins.Count > 0)
            {
                cmb_Plugins.SelectedIndex = 0;
                selectedPlugin = cmb_Plugins.SelectedItem.ToString();
                cmb_Plugins.IsEnabled = true;
            }
        }
        private void ParseJson(string configPath)
        {
            if(File.Exists(configPath))
            {
                StreamReader sr = new StreamReader(configPath);
                string jsonString = sr.ReadToEnd();
                //Debug.WriteLine(jsonString);
                var jsonData = JToken.Parse($"{jsonString}");
                txt_Json.Text = jsonString;
            }
        }
        public void LoadJson(string jsonPath)
        {
            JsonReaderOptions options = new JsonReaderOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };

            string configPath = Path.Combine(jsonPath, tb_ConfigFile.Text);
            if(File.Exists(configPath))
            {
                try
                {
                    Utf8JsonReader reader = new Utf8JsonReader(File.ReadAllBytes(configPath), options);
                    reader.Read();
                    ReadJson(ref reader, TreeItems);
                }
                catch (System.Text.Json.JsonException e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Configuration file cannot be found.");
            }
        }
        public void StringToJson()
        {
            JsonReaderOptions options = new JsonReaderOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };

            byte[] jsonBytes = Encoding.UTF8.GetBytes(txt_Json.Text);
            TreeItems.Clear();

            try
            {
                Utf8JsonReader reader = new Utf8JsonReader(jsonBytes, options);
                reader.Read();
                
                ReadJson(ref reader, TreeItems);
            }
            catch (System.Text.Json.JsonException e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }
        private void ReadJson(ref Utf8JsonReader reader, ObservableCollection<TreeNode> items)
        {
            bool complete = false;
            string propertyName = "";
            while (!complete && reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        propertyName = reader.GetString();
                        Debug.WriteLine("Property Name Called " + propertyName);
                        break;
                    case JsonTokenType.String:
                        items.Add(new TreeValue { Name = propertyName, Value = reader.GetString() });
                        Debug.WriteLine("String Called");
                        break;
                    case JsonTokenType.Number:
                        string number = "";
                        if (reader.TryGetDecimal(out decimal dnumber))
                            number = dnumber.ToString();

                        if (reader.TryGetDouble(out double dbnumber))
                            number = dbnumber.ToString();

                        if (reader.TryGetInt32(out int inumber))
                            number = inumber.ToString();
                        items.Add(new TreeValue { Name = propertyName, Value = number });
                        Debug.WriteLine("Number Called");
                        break;
                    case JsonTokenType.True:
                        items.Add(new TreeValue { Name = propertyName, Value = reader.GetBoolean().ToString() });
                        Debug.WriteLine("TRUE Called");
                        break;
                    case JsonTokenType.False:
                        items.Add(new TreeValue { Name = propertyName, Value = reader.GetBoolean().ToString() });
                        Debug.WriteLine("FALSE Called");
                        break;
                    case JsonTokenType.StartObject:
                        Debug.WriteLine("START OBJECT Called");
                        ObservableCollection<TreeNode> children = new ObservableCollection<TreeNode>();
                        items.Add(new TreeObject { Name = propertyName, Children = children });
                        ReadJson(ref reader, children);
                        break;
                    case JsonTokenType.StartArray:
                        Debug.WriteLine("START ARRAY Called");
                        ObservableCollection<TreeNode> children1 = new ObservableCollection<TreeNode>();
                        items.Add(new TreeObject { Name = propertyName, Children = children1 });
                        ReadJson(ref reader, children1);
                        break;
                    case JsonTokenType.EndArray:
                        complete = true;
                        break;
                    case JsonTokenType.EndObject:
                        complete = true;
                        break;
                    default:
                        Debug.WriteLine("NONE : " + reader.TokenType.ToString() + " PROPERTY NAME: " + propertyName);
                        items.Add(new TreeValue { Name = propertyName, Value = reader.ValueSpan.ToString() });
                        break;
                }
            }
            tv_Json.ItemsSource = TreeItems;
            tv_Json.Items.Refresh();
            tv_Json.IsEnabled = true;
        }

        private async Task WriteObjectJson(string sourceFile)
        {
            JsonWriterOptions options = new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            await using (Stream stream = File.Create(sourceFile))
            {
                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    WriteJsonRecursive(writer, TreeItems);
                }
            }
        }
        private void WriteJsonRecursive(Utf8JsonWriter writer, ObservableCollection<TreeNode> items)
        {
            foreach (TreeNode node in items)
            {
                switch (node)
                {
                    case TreeValue valueNode:
                        writer.WriteString(valueNode.Name.ToString(), valueNode.Value.ToString());
                        break;
                    case TreeObject objectNode:
                        writer.WriteStartObject(objectNode.Name);
                        WriteJsonRecursive(writer, objectNode.Children);
                        break;
                }
            }
            writer.WriteEndObject();
        }

        private void TvApplyChanges()
        {
            WriteObjectJson("tmpConfig.json");
            // Read to for txt_json.text
            ParseJson("tmpConfig.json");
        }

        private void EnableDisableControls()
        {
            if (Maps.Count == 0)
            {
                lb_Maps.IsEnabled = (lb_Maps.Items.Count > 0) ? true : false;
                lb_Excluded.IsEnabled = true;

                btn_IncludeAll.IsEnabled = true;

                btn_ExcludeAll.IsEnabled = false;
                btn_Exclude.IsEnabled = false;
            }
            else
            {
                //Debug.WriteLine($"SELECTED!  {lb_Maps.SelectedIndex}");
                btn_Exclude.IsEnabled = (lb_Maps.SelectedIndex < 0) ? false : true;
                btn_ExcludeAll.IsEnabled = (lb_Maps.SelectedIndex < 0) ? false : true;
            }

            if (Excluded.Count == 0)
            {
                lb_Maps.IsEnabled = (lb_Maps.SelectedIndex < 0) ? false : true;
                lb_Excluded.IsEnabled = false;

                btn_Include.IsEnabled = false;
                btn_IncludeAll.IsEnabled = false;

                btn_ExcludeAll.IsEnabled = (lb_Maps.SelectedIndex < 0) ? false : true;
            }
            else
            {
                btn_Include.IsEnabled = (lb_Excluded.SelectedIndex < 0) ? false : true;
                btn_IncludeAll.IsEnabled = true;
            }
        }
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                selectedPath = dialog.SelectedPath;
                tb_StartIn.Text = selectedPath;
                GetMaps();
            }
        }
        private void btn_ExcludeAll_Click(object sender, RoutedEventArgs e)
        {
            foreach(string maps in Maps)
            {
                Excluded.Add(maps);
            }
            Maps.Clear();
            lb_Excluded.ItemsSource = Excluded;
            lb_Excluded.Items.Refresh();
            lb_Maps.ItemsSource = Maps;
            lb_Maps.Items.Refresh();

            EnableDisableControls();
        }
        private void btn_Exclude_Click(object sender, RoutedEventArgs e)
        {
            Excluded.Add(lb_Maps.SelectedItem.ToString());
            Maps.Remove(lb_Maps.SelectedItem.ToString());

            lb_Excluded.ItemsSource = Excluded;
            lb_Excluded.Items.Refresh();
            lb_Maps.ItemsSource = Maps;
            lb_Maps.Items.Refresh();
            if (Excluded.Count > 0)
            {
                lb_Excluded.IsEnabled = true;

                btn_Include.IsEnabled = true;
                btn_IncludeAll.IsEnabled = true;
            }   
            

            if(Maps.Count == 0)
            {
                btn_Exclude.IsEnabled = false;
                btn_ExcludeAll.IsEnabled = false;
            }
        }
        private void btn_IncludeAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (string exc in Excluded)
            {
                Maps.Add(exc);
            }
            Excluded.Clear();
            lb_Excluded.ItemsSource = Excluded;
            lb_Excluded.Items.Refresh();
            lb_Maps.ItemsSource = Maps;
            lb_Maps.Items.Refresh();

            EnableDisableControls();
        }
        private void btn_Include_Click(object sender, RoutedEventArgs e)
        {
            Maps.Add(lb_Excluded.SelectedItem.ToString());
            lb_Maps.ItemsSource = Maps;
            lb_Maps.Items.Refresh();
            Excluded.Remove(lb_Excluded.SelectedItem.ToString());
            lb_Excluded.ItemsSource = Excluded;
            lb_Excluded.Items.Refresh();

            EnableDisableControls();
        }
        private void lb_Maps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableDisableControls();
        }
        private void lb_Excluded_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableDisableControls();
        }
        private void cmb_Plugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPlugin = (sender as ComboBox).SelectedItem.ToString();
            TreeItems.Clear();
            tv_Json.ItemsSource = TreeItems;
            tv_Json.Items.Refresh();
            tv_Json.IsEnabled = false;

            ParseJson($"{combinedPluginDir}\\{selectedPlugin}\\{tb_ConfigFile.Text}");
            LoadJson($"{combinedPluginDir}\\{selectedPlugin}\\");
        }
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            string sourceFile = combinedPluginDir + "\\" + selectedPlugin + "\\"+ tb_ConfigFile.Text;

            WriteObjectJson(sourceFile);

            if(chk_Apply.IsChecked == true)
            {
                // Replicate to all directories
                foreach (var maps in Maps)
                {
                    if (maps != Maps.First())
                    {
                        string destinationPath = selectedPath + "\\" + maps + pluginBaseDir + "\\" + selectedPlugin;
                        File.Copy(sourceFile, Path.Combine(destinationPath, Path.GetFileName(sourceFile)), true);
                    }

                }
            }

            MessageBox.Show("Configuration has been saved.");
            // Reload sources
            ParseJson($"{combinedPluginDir}\\{selectedPlugin}\\{tb_ConfigFile.Text}");
        }


        private void tb_value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb_value = (sender as TextBox);

            btn_Save.IsEnabled = (tb_value.Text == null) ? false: true;
            tb_value.IsEnabled = (tb_value.Text != null) ? true : false;
            btn_Apply.IsEnabled = true;
        }

        private void txt_Json_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((sender as TextBox).Text != null)
            {
                RawJsonHasChanged = true;
                btn_Save.IsEnabled = true;
            }
        }
        private void txt_Json_LostFocus(object sender, RoutedEventArgs e)
        {
            if(RawJsonHasChanged)
            {
                StringToJson();
                //MessageBox.Show("Triggered changed!" + (sender as TextBox).Text);
            }
        }

        private void btn_Apply_Click(object sender, RoutedEventArgs e)
        {
            TvApplyChanges();
            MessageBox.Show("Settings applied.");
        }

        private void chk_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (chk_Apply.IsChecked == true)
            {
                lb_Maps.SelectedIndex = -1;
                lb_Maps.IsEnabled = false;
                lb_Excluded.IsEnabled = false;

                btn_ExcludeAll.IsEnabled = false;
                btn_Exclude.IsEnabled = false;
                btn_Include.IsEnabled = false;
                btn_IncludeAll.IsEnabled = false;
            }
            else
            {
                lb_Maps.SelectedIndex = 0;
                EnableDisableControls();
            }
        }
        private void gd_raw_GotFocus(object sender, RoutedEventArgs e)
        {
            TvApplyChanges();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // Delete tmpConfig.json
            if(File.Exists("tmpConfig.json"))
                File.Delete(@"tmpConfig.json");
        }

        
    }
}
