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
    /// C数组生成
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// C数组文件生成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Build_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (File_Tree_TV.Items.Count <= 0)
                return;
            if (Path_CBX.SelectedIndex < 0)
                return;
            if (Build_Name_TBX.Text == "")
                return;
            if (TfaDictionary.Count <= 0)
                return;
            string Build_Path = $@"{Path_CBX.Text}{Build_Name_TBX.Text}.h";
            if (File.Exists(Build_Path))
                File.Delete(Build_Path);
            StringBuilder WriteStr = new StringBuilder();
            WriteStr.AppendLine($@"#ifndef {Build_Name_TBX.Text.ToUpper()}_H");
            WriteStr.AppendLine($"#define {Build_Name_TBX.Text.ToUpper()}_H\r\n");
            foreach (Tfa item in TfaDictionary.Values)
            {
                if (item.Width == 0 || item.Height == 0)
                    continue;
                WriteStr.AppendLine(item.ToCArray().ToString());
                WriteStr.AppendLine();
            }
            WriteStr.AppendLine("#endif");
            File.AppendAllText(Build_Path, $"{WriteStr}");
        }
    }
}
