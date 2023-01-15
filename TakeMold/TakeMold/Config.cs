using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Data.SqlTypes;

namespace TakeMold
{
    public partial class MainWindow
    {
        /// <summary>
        /// 文件选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tim = (TreeViewItem)sender;
            Tfa tfa = TfaDictionary[(string)tim.Tag];
            FilePath_LB.Content = $@"文件路径：{tfa.FullName}";
            Name_TB.Text= tfa.Name;
            FileSize_LB.Content = $@"文件大小：{tfa.FileSize / 1024}KB";
            FileType_LB.Content = $@"文件类型：{tfa.FileType}";
            SizeRatio_CB.SelectedValue = "原始配置";
            Exegesis_TB.Text = tfa.Exegesis;
            Modifier_TB.Text = tfa.Modifiers;
            ReConfig();
            if (tfa.TfaType == TfaType_e.RAW)
            {
                SizeRatio_LB.IsEnabled = false;
                SizeRatio_CB.IsEnabled = false;
                RawWidth_LB.IsEnabled = false;
                RawHeight_LB.IsEnabled = false;
                Ratio_LB.IsEnabled = false;
                Ratio_CB.IsEnabled = false;
                Width_LB.IsEnabled = false;
                Width_TB.IsEnabled = false;
                Height_LB.IsEnabled = false;
                Frame_Rate_LB.IsEnabled = false;
                Frames_LB.IsEnabled = false;
                BackGround_CB.IsEnabled = false;
                BackGround_LB.IsEnabled = false;
                IsStretch_LB.IsEnabled = false;
                IsStretch_CB.IsEnabled = false;
                return;
            }
            if (tfa.TfaType == TfaType_e.JPEG || tfa.TfaType == TfaType_e.GIF)
            {
                RawWidth_LB.Content = $@"原始宽度：{tfa.RawWidth}";
                RawHeight_LB.Content = $@"原始高度：{tfa.RawHeight}";
                Width_TB.Text = tfa.Width.ToString();
                Height_TB.Text = tfa.Height.ToString();
            }
            if (tfa.TfaType == TfaType_e.GIF)
            {
                Frame_Rate_LB.Content = $@"GIF帧率：{tfa.Frame_Rate}";
                Frames_LB.Content = $@"GIF帧数：{tfa.Frames}";
            }
            else
            {
                Frame_Rate_LB.IsEnabled = false;
                Frames_LB.IsEnabled = false;
            }
        }
        /// <summary>
        /// 联动比例事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ratio_CB_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)File_Tree_TV.SelectedItem;
            if (tvi == null)
                return;
            if (Ratio_CB.IsChecked == true)
            {
                IsStretch_CB.IsChecked = true;
                BackGround_CB.IsEnabled = false;
                BackGround_LB.IsEnabled = false;
                Tfa tfa = TfaDictionary[(string)tvi.Tag];
                if (tfa.Width < tfa.Height)
                {
                    double Rw = (double)tfa.RawWidth / (double)tfa.Width;
                    Height_TB.Text = ((int)(tfa.RawHeight / Rw)).ToString();
                }
                else
                {
                    double Rh = (double)tfa.RawHeight / (double)tfa.Height;
                    Width_TB.Text = ((int)(tfa.RawWidth / Rh)).ToString();
                }
            }
            else
            {
                IsStretch_CB.IsChecked = false;
                BackGround_CB.IsEnabled = true;
                BackGround_LB.IsEnabled = true;
            }
        }
        /// <summary>
        /// 拉伸选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsStretch_CB_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)File_Tree_TV.SelectedItem;
            if (tvi == null)
                return;
            TfaDictionary[(string)tvi.Tag].IsStretch = IsStretch_CB.IsChecked == null ? false : (bool)IsStretch_CB.IsChecked;
            if (IsStretch_CB.IsChecked == true)
            {
                BackGround_CB.IsEnabled = false;
                BackGround_LB.IsEnabled = false;
            }
            else
            {
                BackGround_CB.IsEnabled = true;
                BackGround_LB.IsEnabled = true;
            }
        }
        /// <summary>
        /// 背景选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackGround_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)File_Tree_TV.SelectedItem;
            if (tvi == null)
                return;
            TfaDictionary[(string)tvi.Tag].BackGround = BackGroupDictionary[BackGround_CB.SelectedValue.ToString()];
        }
        /// <summary>
        /// 配置添加事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeRatio_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string SeleString = SizeRatio_CB.SelectedValue.ToString();
            if (SeleString == "原始配置")
                return;
            if (ConfigName_TB.Text != "")
                return;
            Width_TB.Text= CfDictionary[SeleString].Width.ToString();
            Height_TB.Text = CfDictionary[SeleString].Height.ToString();
            IsStretch_CB.IsChecked = CfDictionary[SeleString].IsStretch;
            BackGround_CB.SelectedValue = CfDictionary[SeleString].BackGround;
            OutPutType_CB.Text=CfDictionary[SeleString].OutPutType;
            Exegesis_TB.Text=CfDictionary[SeleString].Exegesis;
            Modifier_TB.Text=CfDictionary[SeleString].Modifier;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigSave_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigName_TB.Text == "")
                return;
            if (!CfDictionary.ContainsKey(ConfigName_TB.Text))
            {
                CfDictionary.Add(ConfigName_TB.Text, new Configure(ConfigName_TB.Text));
            }
            CfDictionary[ConfigName_TB.Text].Width = int.Parse(Width_TB.Text);
            CfDictionary[ConfigName_TB.Text].Height = int.Parse(Height_TB.Text);
            CfDictionary[ConfigName_TB.Text].IsStretch = IsStretch_CB.IsChecked==true;
            CfDictionary[ConfigName_TB.Text].BackGround = BackGround_CB.Text;
            CfDictionary[ConfigName_TB.Text].OutPutType = OutPutType_CB.Text;
            CfDictionary[ConfigName_TB.Text].Exegesis = Exegesis_TB.Text;
            CfDictionary[ConfigName_TB.Text].Modifier = Modifier_TB.Text;
            SizeRatio_CB.SelectedValue = ConfigName_TB.Text;
            ConfigName_TB.Text = "";
        }
        /// <summary>
        /// 初始输出格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPutType_CB_Loaded(object sender, RoutedEventArgs e)
        {
            OutPutType_CB.ItemsSource = Enum.GetNames(typeof(ImageType_e));
            OutPutType_CB.SelectedIndex = 0;
        }
        /// <summary>
        /// 输出格式选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPutType_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)File_Tree_TV.SelectedItem;
            if (tvi == null)
                return;
            TfaDictionary[(string)tvi.Tag].ImageType = (ImageType_e)Enum.Parse(typeof(ImageType_e), OutPutType_CB.SelectedValue.ToString());
            TfaDictionary[(string)tvi.Tag].FlushName();
            Name_TB.Text = TfaDictionary[(string)tvi.Tag].Name;
        }
        /// <summary>
        /// 重置配置页
        /// </summary>
        private void ReConfig()
        {
            SizeRatio_LB.IsEnabled = true;
            SizeRatio_CB.IsEnabled = true;
            RawWidth_LB.IsEnabled = true;
            RawWidth_LB.Content = $@"原始宽度：";
            RawHeight_LB.IsEnabled = true;
            RawHeight_LB.Content = $@"原始高度：";
            Ratio_LB.IsEnabled = true;
            Ratio_CB.IsEnabled = true;
            Width_LB.IsEnabled = true;
            Width_TB.IsEnabled = true;
            Width_TB.Text = "";
            Height_LB.IsEnabled = true;
            Height_TB.Text = "";
            Frame_Rate_LB.IsEnabled = true;
            Frame_Rate_LB.Content = $@"GIF帧率：";
            Frames_LB.IsEnabled = true;
            Frames_LB.Content = $@"GIF帧数：";
            BackGround_CB.IsEnabled = true;
            BackGround_LB.IsEnabled = true;
            IsStretch_LB.IsEnabled = true;
            IsStretch_CB.IsEnabled = true;
        }
    }
}
