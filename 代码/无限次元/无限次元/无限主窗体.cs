global using System.Diagnostics;
global using 图像处理;
global using 控件;
using System.Drawing.Imaging;
using System.Reflection;

//無限次元:https://space.bilibili.com/2139404925
//本代码来自:https://github.com/becomequantum/Kryon
namespace 无限次元;
public partial class 无限主窗体 : Form {
    高精度计时 计时器 = new();
    图片页 当前图片页;
    图片页 上个图片页;
    int 缩放倍数 = 4;
    public 无限主窗体() {
        InitializeComponent();
        图片页.局部放大控件 = 傅里叶展开标签页.局部放大控件 = 局部放大图1;
        图片页.功能页 = 功能标签页1;
    }
    private void 添加新图片页(Bitmap 位图, string 文件名, ImageFormat 图片格式 = null) {
        if (位图 == null) return;
        图片页 新图片页 = new(位图, 文件名, 图片右键菜单, 缩略图片框,图片格式);
        新图片页.图片框.MouseDoubleClick += 图片框_MouseDoubleClick;
        主标签页.Controls.Add(新图片页);
        主标签页.SelectedTab = 新图片页;
    }

    #region 按键响应
    private void 链接1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        Process.Start("explorer.exe", @"https://space.bilibili.com/2139404925");
    }
    private void 关闭所有图片页ToolStripMenuItem_Click(object sender, EventArgs e) {
        foreach (var 页 in 主标签页.TabPages) 
            if (页 is 图片页) ((图片页)页).关闭();
    }
    private void 主标签页_DoubleClick(object sender, EventArgs e) {
        int i = 主标签页.SelectedIndex;
        if (主标签页.SelectedTab is I标签页 i1) 
            i1.关闭();
        else {
            主标签页.SelectedTab.Dispose();
            return;
        }
        if (i > 1) 主标签页.SelectedIndex = i - 1; //关闭后把选中的标签页设为前一个
        

    }//双击关闭

    private void 图片框_MouseDoubleClick(object sender, MouseEventArgs e) {
        功能标签页1.设置颜色(当前图片页.图数据.位图.GetPixel(e.Location.X, e.Location.Y));
    }//双击图片时取色到界面上的RGB文本框中

    private void 无限主窗体_KeyDown(object sender, KeyEventArgs e) {
        if (当前图片页 == null) return;
        Point 增量 = new (0, 0);
        int 缩放增量 = 0;
        switch (e.KeyData) {
            case Keys.Escape:
                break;
            case Keys.Left:
            case Keys.A:
                增量.X = -1; break;
            case Keys.Up:
            case Keys.W:
                增量.Y = -1; break;
            case Keys.Right:
            case Keys.D:
                增量.X = 1; break;
            case Keys.S:
            case Keys.Down:
                增量.Y = 1; break;
            case Keys.Oemplus:
                缩放增量 = 1; break; //图片放大
            case Keys.OemMinus:
                缩放增量 = -1; break;  //图片缩小
            case Keys.D3:
                当前图片页.显示网格 = !当前图片页.显示网格; 当前图片页.刷新(); break;//放大时加网格线
            case Keys.D0:
                缩放增量 = (int)-(当前图片页.缩放比例 - 1) / 缩放倍数; break;        //回到1:1
            default:
                break;
        }
        if (当前图片页.缩放比例 <= 1)
            Cursor.Position = new Point(Cursor.Position.X + 增量.X, Cursor.Position.Y + 增量.Y); //WASD或方向键控制鼠标上下左右移动一个像素
        else if(增量.X != 0 || 增量.Y != 0)            
            当前图片页.刷新(增量.X * 8, 增量.Y * 8);   //放大模式下方向键控制图片上下移动
        if (缩放增量 != 0) {
            if (WindowState != FormWindowState.Maximized) WindowState = FormWindowState.Maximized;
            当前图片页.图片缩放(缩放增量 * 缩放倍数); 
        }
    }//窗体的KEYPeview属性要设为true才会响应按键.

    private void 打开图片ToolStripMenuItem_Click(object sender, EventArgs e) {
        string[] 图片文件 = 文件.打开文件("打开图片文件(可多选):", "图片文件|*.bmp;*.jpg;*.png;*.jpeg;*.jpe");
        if (图片文件 == null) return;
        计时开始();
        foreach (var 文件名 in 图片文件) {
            Bitmap 位图 = 文件.加载图片文件(文件名, out ImageFormat 图片格式);
            添加新图片页(位图, 文件名, 图片格式);
        }
        计时结束();
    }

    private void 覆盖原图ToolStripMenuItem_Click(object sender, EventArgs e) {
        if (当前图片页 == null) { MessageBox.Show("当前不是图片页!"); return; }
        if (!File.Exists(当前图片页.文件名)) { MessageBox.Show("当前图片还没保存过,请用'另存为'!"); return; }
        文件.保存图片(当前图片页.图数据.位图, 当前图片页.文件名, 当前图片页.图片格式);
    }

    private void 回到原图ToolStripMenuItem_Click(object sender, EventArgs e) {
        if (当前图片页 != null) 当前图片页.回到原图();
        局部放大图1.重置HSL上下限();
    }

    private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e) {
        if (当前图片页 == null) { MessageBox.Show("当前不是图片页!"); return; }
        string 文件名 = 文件.获取保存文件名("另存图片:", "BMP文件(*.bmp) | *.bmp|JPG文件(*.jpg) | *.jpg|PNG文件(*.png) | *.png| ICO文件(*.ico) | *.ico");
        ImageFormat 图片格式  = null;
        if (文件名 != null)  图片格式 = 文件.根据文件格式保存图片(当前图片页.图数据.位图, 文件名);
        if(图片格式 != null) 当前图片页.获取文件信息(文件名, 图片格式);
        标题栏显示信息(当前图片页.文件信息);
    }

    private void 主标签页_SelectedIndexChanged(object sender, EventArgs e) {
        if (主标签页.SelectedTab is 图片页) {
            上个图片页 = 当前图片页;
            当前图片页 = (图片页)主标签页.SelectedTab;
            标题栏显示信息(当前图片页.文件信息);
            当前图片页.切换到此页();
        }
        else {
            当前图片页 = null;
            标题栏显示信息("");
            缩略图片框.Visible = false;
        }
    }//切换图片页时在窗口最上面的标题栏上显示图片信息

    private void 粘贴图片ToolStripMenuItem_Click(object sender, EventArgs e) {
        if (!Clipboard.ContainsImage()) { MessageBox.Show("粘贴板里没有图片!"); return; }
        Bitmap 粘贴板图 = (Bitmap)Clipboard.GetImage();
        添加新图片页(粘贴板图.Clone(new Rectangle(0, 0, 粘贴板图.Width, 粘贴板图.Height), PixelFormat.Format24bppRgb), "粘贴图");
    }

    private void 复制当前页到新页ToolStripMenuItem_Click(object sender, EventArgs e) {
        if(当前图片页 == null) { 标题栏显示信息("还没有打开图片!"); return; }
        添加新图片页(new Bitmap(当前图片页.图数据.位图), "副本 " + 当前图片页.无路径文件名, 当前图片页.图片格式);
    }

    private void 复制选中区域到新页ToolStripMenuItem_Click(object sender, EventArgs e) {
        Bitmap 位图 = 获取选中区域();
        if (位图 != null) 添加新图片页(位图, "(" + 当前图片页.选中区域.Location.X + "," + 当前图片页.选中区域.Location.Y + ") " + 当前图片页.无路径文件名, 当前图片页.图片格式);
    }

    private void 复制选中到粘贴板ToolStripMenuItem_Click(object sender, EventArgs e) {
        Bitmap 位图 = 获取选中区域();
        if (位图 != null) Clipboard.SetImage(位图);
    }

    private void 复制当前页到粘贴板ToolStripMenuItem_Click(object sender, EventArgs e) {
        if (当前图片页 == null) { 标题栏显示信息("还没有打开图片!"); return; }
        Clipboard.SetImage(当前图片页.图数据.位图);
    }
    private void 放大此处ToolStripMenuItem_Click(object sender, EventArgs e) {
        if (WindowState != FormWindowState.Maximized) WindowState = FormWindowState.Maximized;
        当前图片页.图片缩放(缩放倍数, true);
    }
    private void 展开ToolStripMenuItem_Click(object sender, EventArgs e) {
        ((傅里叶展开标签页)主标签页.SelectedTab).展开();
    }

    private void 清空选择ToolStripMenuItem_Click(object sender, EventArgs e) {
        ((傅里叶展开标签页)主标签页.SelectedTab).清空选择();
    }

    private Bitmap 获取选中区域() {
        if(当前图片页 == null) { 标题栏显示信息("还没有打开图片呢!"); return null; }
        if (当前图片页.选中区域.Width <= 2 || 当前图片页.选中区域.Height <= 2) { MessageBox.Show("请先按住右键移动鼠标画框!"); return null;}
        Bitmap 选中图 = new (当前图片页.选中区域.Width, 当前图片页.选中区域.Height, PixelFormat.Format24bppRgb);
        Graphics g选中图 = Graphics.FromImage(选中图);
        g选中图.DrawImage(当前图片页.图数据.位图, 0, 0, 当前图片页.选中区域, GraphicsUnit.Pixel);
        g选中图.Dispose();
        return 选中图;
    }

    private void 功能标签页1_图像处理按钮点击(object sender, EventArgs e) {
        string 功能 = ((Button)sender).Text;
        if (功能 == "刷空图" || 功能 == "随机刷") {
            Bitmap 位图 = new(功能参数.宽, 功能参数.高, 功能参数.像素格式);
            string 文件名 = "空图";
            if (功能 == "随机刷") 文件名 = "随机图";
            添加新图片页(位图, 文件名);
        }
        if (功能 == "三角波" || 功能 == "方波" || 功能 == "金拱门") {
            byte[] 展开数据 = new byte[64] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 80, 150, 187, 215, 234, 244, 250, 248, 239, 220, 179, 123, 56, 56, 123, 179, 220, 239, 248, 250, 244, 234, 215, 187, 150, 80, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 }; //金拱门数据
            int 半周期 = 展开数据.Length / 4;
            double 斜率 = 255.0 / 半周期;
            for (int i = 0; i < 半周期; i++) {
                if (功能 == "方波") {
                    展开数据[i + 半周期] = 240;
                    展开数据[i + 半周期 * 2] = 10;
                    展开数据[i + 半周期 * 3] = 240;
                }
                if (功能 == "三角波") {
                    展开数据[i] = 展开数据[i + 半周期 * 2] = (byte)(i * 斜率 + 0.5);
                    展开数据[i + 半周期] = 展开数据[i + 半周期 * 3] = (byte)(i * -斜率 + 0.5 + 255);
                }
            }
            傅里叶展开标签页 展开页 = new(傅里叶右键菜单, 展开数据, 功能);
            主标签页.Controls.Add(展开页);
            主标签页.SelectedTab = 展开页;

        }
        if (主标签页.SelectedTab is not 图片页) return;
        if (功能 == "变透明") {
            计时开始((Button)sender);
            当前图片页.图数据.位图.MakeTransparent(功能参数.颜色);
            计时结束((Button)sender);
            当前图片页.更新信息();
            标题栏显示信息(当前图片页.文件信息);
        }
        else if (功能 == "刷空图") {
            Graphics g = Graphics.FromImage(当前图片页.图数据.位图);
            计时开始((Button)sender);
            g.Clear(功能参数.颜色); //gdi刷的更快,应该有硬件加速
            计时结束((Button)sender);
            g.Dispose();
        }
        else if (功能 == "二图相减") {
            if (当前图片页 == null || 上个图片页 == null) { MessageBox.Show("还没选中两个图片页"); return; }
            if (!图像.尺寸相等(当前图片页.图数据.位图, 上个图片页.图数据.位图)) { MessageBox.Show("不支持尺寸不同的图片相减!"); return; }
            Bitmap 位图 = new(当前图片页.图数据.位图.Width, 当前图片页.图数据.位图.Height, PixelFormat.Format24bppRgb);
            计时开始((Button)sender);
            图像.位图相减(当前图片页.图数据.位图, 上个图片页.图数据.位图, 位图);
            计时结束((Button)sender);
            添加新图片页(位图, 当前图片页.无路径文件名 + " 减 " + 上个图片页.无路径文件名);
        }
        else if (功能 == "直方图") {
            计时开始((Button)sender);
            Bitmap 直方图 = 当前图片页.图数据.画直方图();
            计时结束((Button)sender);
            添加新图片页(直方图, 当前图片页.无路径文件名 + " 的直方图");
        }
        else if (功能 == "均衡化") {
            计时开始((Button)sender);
            当前图片页.图数据.直方图均衡化();
            计时结束((Button)sender);
        }
        else if (功能 == "标背景" || 功能 == "二值化") {
            计时开始((Button)sender);
            图像.标背景或二值化(当前图片页.图数据.位图, 功能 == "二值化");
            计时结束((Button)sender);
        }
        else if (功能 == "Sobel") {
            计时开始((Button)sender);
            图像.Sobel(当前图片页.图数据.位图);
            计时结束((Button)sender);
        }
        else if (功能 == "递归连通") {
            计时开始((Button)sender);
            图像.递归连通(当前图片页.图数据.位图, 当前图片页.图片框);
            计时结束((Button)sender);
        }
        else
            try {
                Action<Bitmap> 方法委托 = (Action<Bitmap>)Delegate.CreateDelegate(typeof(Action<Bitmap>), typeof(图像).GetMethod(功能), true);
                计时开始((Button)sender);
                方法委托(当前图片页.图数据.位图);
                计时结束((Button)sender);
            }
            catch {
                图像.RefAction<byte, byte, byte> 方法委托 = (图像.RefAction<byte, byte, byte>)Delegate.CreateDelegate(typeof(图像.RefAction<byte, byte, byte>), typeof(图像).GetMethod(功能), true);
                计时开始((Button)sender);
                图像.多功能处理(当前图片页.图数据.位图, 功能, 方法委托);
                计时结束((Button)sender);
            }
        当前图片页.刷新();
    }//图像处理诸多功能按钮响应

    private void 功能标签页1_神经网络按钮点击(object sender, EventArgs e) {
        string 功能 = ((Button)sender).Text;
        if (功能 == "Mnist查看") {
            Mnist查看标签页 新Mnist页 = new();
            主标签页.Controls.Add(新Mnist页);
            主标签页.SelectedTab = 新Mnist页;
            return;
        }
        if (功能 == "读Yolo标签") {
            if (主标签页.SelectedTab is not 图片页) { MessageBox.Show("请先打开一张图片再打开标签txt!"); return; }
            string[] 标签文件 = 文件.打开文件("打开一个Yolo标签txt", "文本文件|*.txt", false);
            图像.读Yolo标签(当前图片页.图数据.位图, 标签文件[0]);
        }
        当前图片页.刷新();
    }

    #endregion

    #region 计时
    //無限次元:https://space.bilibili.com/2139404925
    //本代码来自:https://github.com/becomequantum/Kryon
    private void 计时开始(Control 按钮 = null) {
        if(按钮 != null) 按钮.Enabled = false;
        计时器.清零();
        计时器.开始();

    }

    private void 计时结束(Control 按钮 = null) {
        计时器.停止();
        信息显示.Text = 计时器.时长.ToString("####.##") + "毫秒";
        if (按钮 != null) 按钮.Enabled = true;
    }
    #endregion

    private void 标题栏显示信息(string 信息) {
        Text = "無限次元   " + 信息;
    }

   
}
