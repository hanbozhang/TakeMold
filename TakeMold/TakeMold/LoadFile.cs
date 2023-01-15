using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TakeMold
{
    public partial class MainWindow
    {
        /// <summary>
        /// 文件移入并添加事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            Array paths = ((Array)e.Data.GetData(DataFormats.FileDrop));
            int one = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths.GetValue(i).ToString();
                TreeViewItem tim = LoadFile(path);
                if (tim == null)
                {
                    continue;
                }
                if (one == 0)
                {
                    tim.IsSelected = true;
                    TreeViewItem_Selected(tim, null);
                }
                one++;
            }
        }
        /// <summary>
        /// 文件加载函数
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private TreeViewItem LoadFile(string path)
        {
            if (path == null)
                return null;
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                return null;
            string extension = fileInfo.Extension.ToString().ToUpper().Remove(0, 1);
            if (!Tfa.IsTfaFlile(extension))
                return null;
            if (TfaDictionary.ContainsKey(path))
                return null;
            TfaDictionary.Add(path, new Tfa(fileInfo));
            TreeViewItem tim = new TreeViewItem();
            tim.Header = fileInfo.Name;
            tim.Tag = fileInfo.FullName;
            tim.Selected += TreeViewItem_Selected;
            File_Tree_TV.Items.Add(tim);
            return tim;
        }
    }
}
