using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace TakeMold
{
    /// <summary>
    /// 转换类
    /// </summary>
    public class Tfa
    {
        static public List<string> JPEGImage_Filtering = new List<string>() { "BMP", "JPG", "PNG" };
        static public List<string> AutoImage_Filtering = new List<string>() { "GIF" };
        static public List<string> RawImage_Filtering = new List<string>() { "ICO" };
        static public List<string> WEB_Filtering = new List<string>() { "CSS", "JS", "HTML", "HTM" };
        /// <summary>
        /// 判断是否符合转换条件
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public bool IsTfaFlile(string value)
        {
            return (JPEGImage_Filtering.Exists(t => t == value) || RawImage_Filtering.Exists(t => t == value) || AutoImage_Filtering.Exists(t => t == value) || WEB_Filtering.Exists(t => t == value));
        }

        #region 文件相关属性
        /// <summary>
        /// 文件操作类
        /// </summary>
        public FileInfo file;
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FullName { get; private set; }
        /// <summary>
        /// 数组名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; private set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; private set; }
        #endregion

        #region 图片相关属性
        /// <summary>
        /// 图片输出类型
        /// </summary>
        public ImageType_e ImageType { get; set; } = ImageType_e.JPG;
        /// <summary>
        /// 转换类型
        /// </summary>
        public TfaType_e TfaType { get; private set; }
        /// <summary>
        /// 原始宽度
        /// </summary>
        public int RawWidth { get; private set; }
        /// <summary>
        /// 原始高度
        /// </summary>
        public int RawHeight { get; private set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 是否拉伸图像
        /// </summary>
        public bool IsStretch { get; set; }
        /// <summary>
        /// 背景色
        /// </summary>
        public string BackGround { get; set; } = "0x00000000";
        #endregion

        #region GIF相关属性
        /// <summary>
        /// 帧率
        /// </summary>
        public int Frame_Rate { get; private set; }
        /// <summary>
        /// 帧数
        /// </summary>
        public int Frames { get; private set; }
        #endregion

        #region C数组相关属性
        /// <summary>
        /// 注释
        /// </summary>
        public string Exegesis { get; set; }
        /// <summary>
        /// 修饰符
        /// </summary>
        public string Modifiers { get; set; } = "static const char";

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileInfo">文件属性</param>
        public Tfa(FileInfo fileInfo)
        {
            file = fileInfo;
            FullName = fileInfo.FullName;
            FileType = fileInfo.Extension.ToString().ToUpper().Remove(0, 1);
            Name = $@"{fileInfo.Name.ToUpper().Split('.')[0]}_{FileType}";
            FileSize = fileInfo.Length;
            
            #region 获取JPEG信息
            if (JPEGImage_Filtering.Exists(t => t == FileType))
            {
                TfaType = TfaType_e.JPEG;
                Image image = Image.FromFile(fileInfo.FullName);
                RawWidth = image.Width;
                RawHeight = image.Height;
                Width = image.Width;
                Height = image.Height;
            }
            #endregion

            #region 获取原始文件信息
            if (RawImage_Filtering.Exists(t => t == FileType) || WEB_Filtering.Exists(t => t == FileType))
                TfaType = TfaType_e.RAW;
            #endregion

            #region 获取GIF信息
            if (AutoImage_Filtering.Exists(t => t == FileType))
            {
                TfaType = TfaType_e.GIF;
                using (Image image = Image.FromFile(fileInfo.FullName))
                {
                    RawWidth = image.Width;
                    RawHeight = image.Height;
                    Width = image.Width;
                    Height = image.Height;
                    FrameDimension gf = new FrameDimension(image.FrameDimensionsList[0]);
                    Frames = image.GetFrameCount(gf);
                    if (Frames > 1)
                    {
                        bool stop = false;
                        for (int i = 0; i < Frames; i++)//遍历图像帧
                        {
                            if (stop == true)
                                break;
                            image.SelectActiveFrame(gf, i);//激活当前帧
                            for (int j = 0; j < image.PropertyIdList.Length; j++)//遍历帧属性
                            {
                                if ((int)image.PropertyIdList.GetValue(j) == 0x5100)//如果是延迟时间
                                {
                                    PropertyItem pItem = (PropertyItem)image.PropertyItems.GetValue(j);//获取延迟时间属性
                                    byte[] delayByte = new byte[4];//延迟时间，以1/100秒为单位
                                    delayByte[0] = pItem.Value[i * 4];
                                    delayByte[1] = pItem.Value[1 + i * 4];
                                    delayByte[2] = pItem.Value[2 + i * 4];
                                    delayByte[3] = pItem.Value[3 + i * 4];
                                    Frame_Rate = BitConverter.ToInt32(delayByte, 0) * 10; //乘以10，获取到毫秒
                                    stop = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Frame_Rate = 0;
                    }
                }
            }
            #endregion
            Name = $@"{ImageType}_{Name}";
            //名称注释
            Exegesis = $"Name: {Name}_HEX\r\nDescribe:";

        }

        /// <summary>
        /// 转换C数组
        /// </summary>
        /// <returns>转换的字符串组</returns>
        public StringBuilder ToCArray()
        {
            StringBuilder ret = new StringBuilder();
            switch (TfaType) {
                case TfaType_e.RAW:
                    RAW_ToCArray(ref ret);
                    break;
                case TfaType_e.GIF:
                    GIF_ToCArray(ref ret);
                    break;
                case TfaType_e.JPEG:
                    JPEG_ToCArray(ref ret);
                    break;
            }
            return ret;
        }
        /// <summary>
        /// 原始文件转C数组
        /// </summary>
        /// <param name="ret">返回字符串组</param>
        private void RAW_ToCArray(ref StringBuilder ret)
        {
            //添加长度定义
            ret.AppendLine($@"#define {Name}_Hex_Len {FileSize}");
            //添加注释
            foreach (string item in Exegesis.Split('\n', '\r'))
            {
                ret.AppendLine($@"// {item}");
            }
            ret.AppendLine($@"{Modifiers} {Name}_Hex[{Name}_Hex_Len]={{");
            if (!file.Exists)
                return;
            file.Refresh();
            FileStream fileStream = file.OpenRead();
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            fileStream.Close();
            ToHexString(ref ret,ref bytes);
        }
        /// <summary>
        /// JPEG格式转C数组
        /// </summary>
        /// <param name="ret">返回字符串组</param>
        private void JPEG_ToCArray(ref StringBuilder ret)
        {
            string SavePath = $@"{Directory.GetCurrentDirectory()}\\Buffer.bin";
            using (Image image = Image.FromFile(file.FullName))
            {
                Bitmap bmp = new Bitmap(Width,Height, PixelFormat.Format24bppRgb);
                ImageStretch(ref bmp,image);
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                bmp.Save(SavePath, GetImageFormat());
                bmp.Dispose();
            }
            FileStream fileStream = new FileStream(SavePath,FileMode.Open,FileAccess.Read);
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            fileStream.Close();
            //添加长度定义
            ret.AppendLine($@"#define {Name}_Hex_Len {bytes.Length}");
            //添加注释
            foreach (string item in Exegesis.Split('\n', '\r'))
            {
                ret.AppendLine($@"// {item}");
            }
            //添加长宽注释
            ret.AppendLine($@"// Height:{Height}px Width:{Width}px");
            ret.AppendLine($@"{Modifiers} {Name}_Hex[{Name}_Hex_Len]={{");
            ToHexString(ref ret, ref bytes);
        }
        /// <summary>
        /// GIF格式转C数组
        /// </summary>
        /// <param name="ret">返回字符串组</param>
        private void GIF_ToCArray(ref StringBuilder ret)
        {
            ret.AppendLine($@"// {Name} Frames");
            ret.AppendLine($@"#define {Name}_FrameN {Frames}");
            ret.AppendLine($"char * Get_{Name}(char i)\r\n{{");
            ret.AppendLine($"    switch (i)\r\n    {{");
            for (int i = 0; i <Frames; i++)
            {
                ret.AppendLine($"        case {i}:");
                ret.AppendLine($"            return {Name}_{i}_Hex;");
            }
            ret.AppendLine($"    }}\r\n}};");
            string SavePath = $@"{Directory.GetCurrentDirectory()}\\Buffer.bin";
            using (Image gif = Image.FromFile(file.FullName))
            {
                FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
                int count = gif.GetFrameCount(fd);
                for (int i = 0; i < count; i++)
                {
                    gif.SelectActiveFrame(fd, i);
                    Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
                    ImageStretch(ref bmp, gif);
                    if (File.Exists(SavePath))
                    {
                        File.Delete(SavePath);
                    }
                    bmp.Save(SavePath, GetImageFormat());
                    bmp.Dispose();
                    FileStream fileStream = new FileStream(SavePath, FileMode.Open, FileAccess.Read);
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, (int)fileStream.Length);
                    fileStream.Close();
                    //添加长度定义
                    ret.AppendLine($@"#define {Name}_{i}_Hex_Len {bytes.Length}");
                    ret.AppendLine($@"// {Name}_{i}");
                    //添加注释
                    foreach (string item in Exegesis.Split('\n','\r'))
                    {
                        if (item == "")
                            continue;
                        ret.AppendLine($@"// {item}");
                    }
                    //添加长宽注释
                    ret.AppendLine($@"// Height:{Height}px Width:{Width}px");
                    ret.AppendLine($@"{Modifiers} {Name}_{i}_Hex[{Name}_{i}_Hex_Len]={{");
                    ToHexString(ref ret, ref bytes);
                }
            }
        }
        /// <summary>
        /// 二进制转16进制字符
        /// </summary>
        /// <param name="ret">返回字符组</param>
        /// <param name="bytes">二进制数组</param>
        private void ToHexString(ref StringBuilder ret,ref byte[] bytes)
        {
            if (bytes.Length == 0)
                return;
            string row = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                string BH = Convert.ToString(bytes[i], 16).ToUpper();
                if(BH.Length>1)
                    row += $@"0x{BH},";
                else
                    row += $@"0x0{BH},";
                if ((i % 30 == 0)&&(i>15))
                {
                    ret.AppendLine(row);
                    row = "";
                }
            }
            if (row != "")
            {
                ret.AppendLine(row);
            }
            ret = ret.Remove(ret.Length - 3, 3);
            ret.Append("};\r\n\r\n");
        }
        private ImageFormat GetImageFormat()
        {
            ImageFormat format = ImageFormat.Jpeg;
            switch (ImageType)
            {
                case ImageType_e.JPG:
                    format = ImageFormat.Jpeg;
                    break;
                case ImageType_e.PNG:
                    format = ImageFormat.Png;
                    break;
                case ImageType_e.BMP:
                    format = ImageFormat.Bmp;
                    break;
            }
            return format;
        }
        /// <summary>
        /// 图片拉伸
        /// </summary>
        /// <param name="bmp">Bitmap操作对象</param>
        /// <param name="image">拉伸图像</param>
        private void ImageStretch(ref Bitmap bmp,Image image)
        {
            Graphics g = Graphics.FromImage(bmp);
            if (!IsStretch)
            {
                g.DrawImage(image, new Rectangle(0, 0, Width, Height));
            }
            else
            {
                g.Clear(Color.FromArgb(Convert.ToInt32(BackGround, 16)));
                double Rw = (double)RawWidth / (double)Width;
                double Rh = (double)RawHeight / (double)Height;
                int inW, inH;
                if (Rw < Rh)
                {
                    Rw = Rh;
                }
                inW = (int)(RawWidth / Rw);
                inH = (int)(RawHeight / Rw);
                g.DrawImage(image, new Rectangle((Width - inW) / 2, (Height - inH) / 2, inW, inH));
            }
            g.Dispose();
        }
        /// <summary>
        /// 刷新名称
        /// </summary>
        public void FlushName()
        { 
            string[] Names =Name.Split('_');
            Name= $@"{ImageType}_";
            for (int i = 1; i < Names.Length-1; i++)
            {
                Name += $@"{Names[i]}_";
            }
            Name += Names[Names.Length - 1];
        }
    }
    /// <summary>
    /// 转换类型
    /// </summary>
    public enum TfaType_e
    {
        RAW = 0,
        JPEG = 1,
        GIF = 2
    }
    public enum ImageType_e
    { 
        JPG=0,
        PNG=1,
        BMP=3
    }
}
