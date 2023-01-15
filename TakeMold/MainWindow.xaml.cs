using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml;
using System.Xml.Linq;

namespace TakeMold
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, Tfa> TfaDictionary = new Dictionary<string, Tfa>();
        public Dictionary<string, Configure>CfDictionary = new Dictionary<string, Configure>();
        public Dictionary<string, string> BackGroupDictionary = new Dictionary<string, string>();
        public string ConfigPath = $@"{Directory.GetCurrentDirectory()}\Config.xml";
        public XElement XMlConfig;
        public MainWindow()
        {
            InitializeComponent();
            //加载配置文件
            LoadConfig();
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            File_Tree_TV.Items.Clear();
            TfaDictionary.Clear();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)File_Tree_TV.SelectedItem;
            File_Tree_TV.Items.Remove(treeViewItem);
            TfaDictionary.Remove((string)treeViewItem.Tag);
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
        }
        /// <summary>
        /// 文件选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browse_Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog FBD = new System.Windows.Forms.FolderBrowserDialog();
            FBD.Description = "请选择一个路径";
            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Path_CBX.Items.Add(FBD.SelectedPath);
                Path_CBX.SelectedItem = FBD.SelectedPath;
            }
            if (Path_CBX.Items.Count > 5)
            {
                Path_CBX.Items.Remove(Path_CBX.Items[0]);
            }
        }

        /// <summary>
        /// 图像宽度或高度输入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }
        /// <summary>
        /// 输入改变锁
        /// </summary>
        private bool TextLock =true;
        /// <summary>
        /// 输入改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            TreeViewItem tvi = (TreeViewItem)File_Tree_TV.SelectedItem;
            if (tvi == null)
                return;
            Tfa tfa = TfaDictionary[(string)tvi.Tag];
            if (tb.Text == "")
                return;
            switch (tb.Name)
            {
                case "Width_TB":
                    tfa.Width = int.Parse(tb.Text);
                    if (Ratio_CB.IsChecked == true && TextLock)
                    {
                        double Rw = (double)tfa.RawWidth / tfa.Width;
                        int Sw = (int)(tfa.RawHeight / Rw);
                        TextLock = false;
                        Height_TB.Text = Sw.ToString();
                        TextLock = true;
                    }
                    break;
                case "Height_TB":
                    tfa.Height = int.Parse(tb.Text);
                    if (Ratio_CB.IsChecked == true&& TextLock)
                    {
                        double Rh = (double)tfa.RawHeight /tfa.Height;
                        int Sw = (int)(tfa.RawWidth / Rh);
                        TextLock = false;
                        Width_TB.Text = Sw.ToString();
                        TextLock = true;
                    }
                    break;
            }
        }
        /// <summary>
        /// 配置保存事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveConfig();
        }
        /// <summary>
        /// 注释修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exegesis_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)File_Tree_TV.SelectedItem;
            if (tvi == null)
                return;
            Tfa tfa = TfaDictionary[(string)tvi.Tag];
            if (Exegesis_TB.Text == "")
                return;
            tfa.Exegesis = Exegesis_TB.Text;
        }
        /// <summary>
        /// 修饰符
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Modifier_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)File_Tree_TV.SelectedItem;
            if (tvi == null)
                return;
            Tfa tfa = TfaDictionary[(string)tvi.Tag];
            if (Modifier_TB.Text == "")
                return;
            tfa.Modifiers = Modifier_TB.Text;
        }
    }
}
