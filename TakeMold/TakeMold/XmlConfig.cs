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
    /// 配置加载 分文件
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// 配置加载函数
        /// </summary>
        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                NewLoadConfig();
            }
            else
            {
                XDocument xDoc = XDocument.Load(ConfigPath);
                XMlConfig = xDoc.Element("TakeMoldConfig");
                Build_Name_TBX.Text = XMlConfig.Attribute("Name").Value;
                foreach (XElement item in XMlConfig.Elements())
                {
                    if (item.Attribute("Count").Value == "0")
                        continue;
                    if (item.Name == "Files")
                    {
                        LoadConfig_Files(item);
                    }
                    if (item.Name == "Configure")
                    {
                        LoadConfig_Configure(item);
                    }
                    if (item.Name == "SavePaths")
                    {
                        LoadConfig_SavePaths(item);
                    }
                    if (item.Name == "BackGround")
                    {
                        LoadConfig_BackGroup(item);
                    }
                }
            }

        }

        /// <summary>
        /// 配置保存函数
        /// </summary>
        private void SaveConfig()
        {
            File.Delete("ConfigPath");
            int i = 1;
            XMlConfig.Attribute("Name").Value = Build_Name_TBX.Text;
            XMlConfig.Element("Files").Attribute("Count").Value = TfaDictionary.Count.ToString();
            XMlConfig.Element("Files").RemoveNodes();
            foreach (var item in TfaDictionary.Keys)
            {
                XMlConfig.Element("Files").Add(new XElement($@"F{i}", item));
                i++;
            }
            i = 1;
            XMlConfig.Element("Configure").Attribute("Count").Value = CfDictionary.Count.ToString();
            XMlConfig.Element("Configure").RemoveNodes();
            foreach (var item in CfDictionary.Values)
            {
                XMlConfig.Element("Configure").Add(new XElement($@"C{i}", item.Name));
                if (item.Name != "原始配置")
                {
                    XMlConfig.Element("Configure").Element($@"C{i}").Add(new XAttribute("Width", item.Width));
                    XMlConfig.Element("Configure").Element($@"C{i}").Add(new XAttribute("Height",item.Height));
                    XMlConfig.Element("Configure").Element($@"C{i}").Add(new XAttribute("IsStretch",item.IsStretch));
                    XMlConfig.Element("Configure").Element($@"C{i}").Add(new XAttribute("BackGround", item.BackGround));
                    XMlConfig.Element("Configure").Element($@"C{i}").Add(new XAttribute("OutPutType", item.OutPutType));
                    XMlConfig.Element("Configure").Element($@"C{i}").Add(new XAttribute("Exegesis", item.Exegesis));
                    XMlConfig.Element("Configure").Element($@"C{i}").Add(new XAttribute("Modifier", item.Modifier));
                }
                i++;
            }
            i = 1;
            XMlConfig.Element("SavePaths").Attribute("Count").Value = Path_CBX.Items.Count.ToString();
            XMlConfig.Element("SavePaths").RemoveNodes();
            foreach (var item in Path_CBX.Items)
            {
                XMlConfig.Element("SavePaths").Add(new XElement($@"F{i}", item.ToString()));
                i++;
            }
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;
            //创建一个新的xml文件并将编码信息等写入进去
            XmlWriter xw = XmlWriter.Create(ConfigPath, settings);
            //保存xml
            XMlConfig.Save(xw);
            //刷新数据
            xw.Flush();
            //关闭
            xw.Close();
        }
        /// <summary>
        /// 创建新的配置文件
        /// </summary>
        private void NewLoadConfig()
        {
            XElement xEt = new XElement("TakeMoldConfig", new XAttribute("Name", "Resource"));
            xEt.Add(new XElement("Files", new XAttribute("Count", "0")));
            xEt.Add(new XElement("SavePaths", new XAttribute("Count", "0")));
            xEt.Add(new XElement("BackGround", new XAttribute("Count", "2")));
            xEt.Element("BackGround").Add(new XElement("黑色", new XAttribute("Color", "0xFF000000")));
            xEt.Element("BackGround").Add(new XElement("白色", new XAttribute("Color", "0xFFFFFFFF")));
            xEt.Add(new XElement("Configure", new XAttribute("Count", "1")));
            xEt.Element("Configure").Add(new XElement("C1","原始配置"));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;
            //创建一个新的xml文件并将编码信息等写入进去
            XmlWriter xw = XmlWriter.Create(ConfigPath, settings);
            //保存xml
            xEt.Save(xw);
            //刷新数据
            xw.Flush();
            //关闭
            xw.Close();
            LoadConfig();
        }
        /// <summary>
        /// 加载文件
        /// </summary>
        /// <param name="xEt"></param>
        private void LoadConfig_Files(XElement xEt)
        {
            int one = 0;
            foreach (var item in xEt.Elements())
            {
                TreeViewItem tim = LoadFile(item.Value);
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
        /// 加载预设
        /// </summary>
        /// <param name="xEt"></param>
        private void LoadConfig_Configure(XElement xEt)
        {
            foreach (var item in xEt.Elements())
            {
                CfDictionary.Add(item.Value, new Configure(item.Value));
                if (item.Value != "原始配置")
                {
                    CfDictionary[item.Value].Width = int.Parse(item.Attribute("Width").Value);
                    CfDictionary[item.Value].Height=int.Parse(item.Attribute("Height").Value);
                    CfDictionary[item.Value].IsStretch = (item.Attribute("IsStretch").Value=="true");
                    CfDictionary[item.Value].BackGround = item.Attribute("BackGround").Value;
                    CfDictionary[item.Value].OutPutType = item.Attribute("OutPutType").Value;
                    CfDictionary[item.Value].Exegesis = item.Attribute("Exegesis").Value;
                    CfDictionary[item.Value].Modifier = item.Attribute("Modifier").Value;
                }
            }
            SizeRatio_CB.ItemsSource = CfDictionary.Keys;
            SizeRatio_CB.SelectedValue= "原始配置";
        }
        /// <summary>
        /// 加载保存路径
        /// </summary>
        /// <param name="xEt"></param>
        private void LoadConfig_SavePaths(XElement xEt)
        {
            foreach (var item in xEt.Elements())
            {
                Path_CBX.Items.Add(item.Value);
            }
            Path_CBX.SelectedIndex = 0;
        }
        /// <summary>
        /// 加载背景颜色
        /// </summary>
        /// <param name="item"></param>
        private void LoadConfig_BackGroup(XElement xEt)
        {
            foreach (var item in xEt.Elements())
            {
                if (BackGroupDictionary.ContainsKey(item.Name.ToString()))
                    continue;
                BackGroupDictionary.Add(item.Name.ToString(),item.Attribute("Color").Value);
            }
            BackGround_CB.ItemsSource=BackGroupDictionary.Keys;
            BackGround_CB.SelectedIndex= 0;
        }
    }
    /// <summary>
    /// 尺寸配置
    /// </summary>
    public class Configure
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 是否拉伸
        /// </summary>
        public bool IsStretch { get; set; }
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BackGround { get; set; }
        /// <summary>
        /// 输出类型
        /// </summary>
        public string OutPutType { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string Exegesis { get; set; }
        /// <summary>
        /// 修饰符
        /// </summary>
        public string Modifier { get; set; }

        public Configure(string name)
        {
            Name = name;
        }
        
    }
}
