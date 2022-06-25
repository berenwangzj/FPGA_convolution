global using 图像处理;

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 控件;

public interface I标签页 {
    void 关闭();
}
public class 图片页 : TabPage, I标签页 {
    public PictureBox 图片框;
    PictureBox 缩略图框;          //用于放大模式下显示和设置放大区域
    public 图片数据 图数据;
    public string 文件名;
    public string 无路径文件名;
    public string 文件信息;
    public ImageFormat 图片格式;
    static public 局部放大图 局部放大控件;
    static public 功能标签页 功能页;
    public Rectangle 选中区域 = Rectangle.Empty;

    Bitmap 放大图;
    Point 局部中心;
    public float 缩放比例 = 1;
    public bool 显示网格 = false;
    Rectangle 原图局部框;
    Point 右键按下点;
    double 缩略比 = 0;

    Rectangle 虚线框 = Rectangle.Empty;
    Rectangle 上个虚线框 = Rectangle.Empty;
    Point 按下起始点;
    Point 上个点;
    int 彩线计数;

    public 图片页(Bitmap 位图, string 图片文件名, ContextMenuStrip 右键菜单, PictureBox 缩略图框, ImageFormat 格式 = null) {
        this.缩略图框 = 缩略图框;
        图数据 = new(位图);
        if (格式 == null) 格式 = ImageFormat.Bmp;
        获取文件信息(图片文件名, 格式);

        //图片框初始化
        图片框 = new();
        图片框.SizeMode = PictureBoxSizeMode.AutoSize;
        图片框.Image = 位图;
        图片框.Refresh();
        图片框.MouseMove += 图片框_MouseMove;
        图片框.MouseDown += 图片框_MouseDown;
        图片框.MouseUp += 图片框_MouseUp;

        //TabPage初始化
        AutoScroll = true;
        Controls.Add(图片框);
        Resize += 图片页_Resize;
        ContextMenuStrip = 右键菜单;

        缩略图框.MouseMove += 缩略图框_MouseMove;
        缩略图框.MouseUp += 缩略图框_MouseMove;

    }

    public void 回到原图() {
        if (!File.Exists(文件名)) { MessageBox.Show("没保存过的图片回不去!"); return; }
        图数据.位图 = 文件.加载图片文件(文件名, out ImageFormat 图片格式);
        if (缩放比例 == 1) {
            图片框.Image = 图数据.位图;
            图片框.Refresh();
        }
        else if (缩放比例 < 1) {
            图片框.Image = 图数据.位图;
            调到合适大小();
        }
        else {
            刷新局部放大图();
            图片框.Refresh();
        }           


    }

    public void 获取文件信息(string 图片文件名, ImageFormat 格式) {
        string 文件大小 = "";
        if (File.Exists(图片文件名)) {//如果是打开的图片
            文件名 = 图片文件名;
            无路径文件名 = Path.GetFileName(文件名);
            文件大小 = ((new FileInfo(图片文件名)).Length / 1024.0).ToString(".##") + "KB";
        }
        else
            文件名 = 无路径文件名 = 图片文件名;
        Text = 无路径文件名;
        文件信息 = 图数据.位图.Width + " x " + 图数据.位图.Height + "  " + 文件大小 + "  像素格式: " + 图数据.位图.PixelFormat.ToString()[6..] + "   " + 文件名;
        图片格式 = 格式;

    }

    public void 更新信息() {
        获取文件信息(文件名, 图片格式);
    }



    #region 鼠标事件
    private void 缩略图框_MouseMove(object sender, MouseEventArgs e) {
        if (缩略图框.Visible == false) return;
        if (e.Button == MouseButtons.Left) {
            if (图像.点在框内(e.Location, new Rectangle(new Point(0, 0), 缩略图框.Size), 5, 5)) {
                局部中心 = new Point((int)(e.Location.X / 缩略比), (int)(e.Location.Y / 缩略比));
                刷新局部放大图();
                图片框.Refresh();
            }       
        }
    }//在缩略图上可拖动局部放大框的位置
    private void 图片框_MouseMove(object sender, MouseEventArgs e) {
        if (!(缩放比例 > 1 && e.Button == MouseButtons.Left)) {
            Point 鼠标位置 = new((int)(e.Location.X / 缩放比例), (int)(e.Location.Y / 缩放比例));   //这个e.Location是相对于图片框的坐标
            if (缩放比例 > 1) { 鼠标位置.X += 原图局部框.X; 鼠标位置.Y += 原图局部框.Y; }
            局部放大控件.显示像素信息(图数据.位图, 鼠标位置, e.Button == MouseButtons.Left);
        }

        if (e.Button == MouseButtons.Right) {
            虚线框 = new Rectangle(Math.Min(按下起始点.X, Cursor.Position.X), Math.Min(按下起始点.Y, Cursor.Position.Y), Math.Abs(按下起始点.X - Cursor.Position.X), Math.Abs(按下起始点.Y - Cursor.Position.Y));
            ControlPaint.DrawReversibleFrame(上个虚线框, Color.Pink, FrameStyle.Dashed);
            ControlPaint.DrawReversibleFrame(虚线框, Color.Pink, FrameStyle.Dashed);
            上个虚线框 = 虚线框;
        }//按下右键拖动画虚线框

        if (e.Button == MouseButtons.Left) {
            if (缩放比例 <= 1) {
                using Graphics g图片框 = 图片框.CreateGraphics();
                g图片框.DrawLine(new Pen(图像.色谱颜色24[彩线计数++ % 图像.色谱颜色24.Length]), 上个点, e.Location);
                上个点 = e.Location;
                功能页.更新HSL上下限(局部放大控件.HSL上限, 局部放大控件.HSL下限);
            }//按下左键拖动时画彩线
            else {
                int dx = -(int)((e.Location.X - 上个点.X) / 缩放比例 + 0.5);
                int dy = -(int)((e.Location.Y - 上个点.Y) / 缩放比例 + 0.5);
                if (dx != 0 || dy != 0) { //非零时再刷
                    刷新(dx, dy);
                    上个点.X -= dx * (int)缩放比例;
                    上个点.Y -= dy * (int)缩放比例;  //这样赋值'上个点'画面才能跟紧鼠标
                }
            }//放大模式下按住左键可拖动画面
        }
        Focus();                                     //让Tabpage获得焦点好让滚轮能够滚动页面
    }

    private void 图片框_MouseUp(object sender, MouseEventArgs e) {
        图片框.Refresh();
        选中区域 = new Rectangle(图片框.PointToClient(上个虚线框.Location),上个虚线框.Size);
        选中区域 = new Rectangle((int)(选中区域.X / 缩放比例), (int)(选中区域.Y / 缩放比例), (int)(选中区域.Width / 缩放比例), (int)(选中区域.Height / 缩放比例));
        if (缩放比例 > 1) { 选中区域.X += 原图局部框.X; 选中区域.Y += 原图局部框.Y; }
        上个虚线框 = Rectangle.Empty;
    }

    private void 图片框_MouseDown(object sender, MouseEventArgs e) {
        按下起始点 = Cursor.Position;
        上个点 = e.Location;
        彩线计数 = 0;
        if (e.Button == MouseButtons.Right) {
            右键按下点 = new((int)(e.Location.X / 缩放比例), (int)(e.Location.Y / 缩放比例));   //这个e.Location是相对于图片框的坐标
            if (缩放比例 > 1) { 右键按下点.X += 原图局部框.X; 右键按下点.Y += 原图局部框.Y; }
        }
    }
    #endregion

    #region 图片缩放

    public void 切换到此页() {
        if (缩放比例 <= 1)
            缩略图框.Visible = false;
        else
            显示缩略图(); 

    }
    public void 图片缩放(int 增量 = 0, bool 右键放大 = false) {
        if (右键放大 && 图像.点在框内(右键按下点, 图数据.位图)) {
            局部中心 = 右键按下点;
            if (缩放比例 < 1) 缩放比例 = 1;  //好直接进入放大状态
        }
        if (缩放比例 == 1) {
            if (增量 < 0) 调到合适大小(); //1比1时按减号就所小到合适
            if (增量 > 0) {
                缩放比例 += 增量;
                if(!右键放大) 获取局部中心点坐标();
                进入放大模式();
                刷新局部放大图();
                图片框.Refresh();
            }
        }
        else if (缩放比例 < 1) {
            if (增量 > 0) 调到1比1();     //缩小到合适窗口大小状态下按加号就回到1比1
        }
        else {                            //处于放大状态下
            缩放比例 += 增量;
            if (缩放比例 == 1) { 调到1比1();  return; }
            刷新局部放大图();
            图片框.Refresh();
        }
    }
    
    private void 图片页_Resize(object sender, EventArgs e) {
        if (缩放比例 < 1) 调到合适大小();
    }
    private void 调到合适大小() {
        if (图片框.Image.Width <= Size.Width && 图片框.Image.Height <= Size.Height) {
            图片框.SizeMode = PictureBoxSizeMode.AutoSize;
            缩放比例 = 1;
        }
        else {
            图片框.SizeMode = PictureBoxSizeMode.Zoom;
            AutoScrollPosition = new Point(0, 0); //这一句要写在下一句的前面，这两句是为了解决1:1模式下滚动条有滚动后再回到合适模式会产生的图片位置跑偏问题。
            图片框.Location = new Point(0, 0);
            double 图片宽长比 = (double)图片框.Image.Width / 图片框.Image.Height;
            double Tab宽长比 = (double)Size.Width / Size.Height;

            if (图片宽长比 >= Tab宽长比) {  //图片较宽
                int 高 = (int)(Size.Width / 图片宽长比 + 0.5);
                图片框.Size = new Size(Size.Width, 高);    //图片较宽时图片框的宽度会定为和Tab的宽度相同,这样图高不会超过Tab的高度.
            }
            else {
                int 宽 = (int)(Size.Height * 图片宽长比 + 0.5);
                图片框.Size = new Size(宽, Size.Height);
            }
            缩放比例 = (float)图片框.Width / 图片框.Image.Width ;
            AutoScroll = false;
        }

    }//图片比显示区域大时,把图片缩小到适合显示区域的大小,此时没有滚动条,能看到完整图片.

    private void 调到1比1() {
        缩放比例 = 1;
        图片框.SizeMode = PictureBoxSizeMode.AutoSize;
        图片框.Image = 图数据.位图;
        AutoScroll = true;
        图片框.Refresh();
        缩略图框.Visible = false;
    }

    private void 进入放大模式() {
        AutoScroll = false;  //放大模式下也没有滚动条,靠按住鼠标左键来拖动画面
        if (放大图 == null) 放大图 = new (Width, Height, PixelFormat.Format24bppRgb);
        图片框.SizeMode = PictureBoxSizeMode.AutoSize;
        图片框.Image = 放大图;
        显示缩略图();
    }

    private void 显示缩略图() {
        缩略图框.Size = new((int)(缩略图框.Height * (图数据.位图.Width / (double)图数据.位图.Height)), 缩略图框.Height);
        缩略图框.Image = 图数据.位图;
        缩略图框.Visible = true;
        缩略图框.Refresh();
        缩略比 = 缩略图框.Width / (double)图数据.位图.Width;
    }

    private void 刷新局部放大图() {
        int 局部宽 = (int)(放大图.Width / 缩放比例), 局部高 = (int)(放大图.Height / 缩放比例); //放大图的大小就是显示区域的大小,就是this.Size,也就是Tab页的大小
        局部宽 = Math.Min(局部宽, 图数据.位图.Width) + 1;
        局部高 = Math.Min(局部高, 图数据.位图.Height) + 1;         //图片放大之后还比显示区域小的情况
        int 左上X = 局部中心.X - 局部宽 / 2 - 1, 左上Y = 局部中心.Y - 局部高 / 2 - 1;
        if (左上X < 0) { 左上X = 0; }
        if (左上Y < 0) { 左上Y = 0; } //确保左上点不小于零
        原图局部框 = new Rectangle(左上X, 左上Y, 局部宽, 局部高);
        if (原图局部框.Right > 图数据.位图.Width) 原图局部框.Width = 图数据.位图.Width - 原图局部框.Left;
        if (原图局部框.Bottom > 图数据.位图.Height) 原图局部框.Height = 图数据.位图.Height - 原图局部框.Top; //确保右上点不超过原图边界

        BitmapData 位图数据 = 图数据.位图.LockBits(new Rectangle(0,0,图数据.位图.Width,图数据.位图.Height), ImageLockMode.ReadWrite, 图数据.位图.PixelFormat);
        byte 字节数 = 3;
        if (图数据.位图.PixelFormat == PixelFormat.Format32bppArgb) 字节数 = 4;
        using Graphics g = Graphics.FromImage(放大图);
        g.Clear(BackColor);
        unsafe {
            byte* 位针 = (byte*)(位图数据.Scan0);
            for (int y = 原图局部框.Top; y < 原图局部框.Bottom; y++) {
                for (int x = 原图局部框.Left; x < 原图局部框.Right; x++) {
                    Color 颜色 = Color.FromArgb(位针[y * 位图数据.Stride + x * 字节数 + 图像.红], 位针[y * 位图数据.Stride + x * 字节数 + 图像.绿], 位针[y * 位图数据.Stride + x * 字节数 + 图像.蓝]);
                    g.FillRectangle(new SolidBrush(颜色), (x - 原图局部框.Left) * 缩放比例, (y - 原图局部框.Top) * 缩放比例, 缩放比例, 缩放比例);
                    
                }
            }           
            图数据.位图.UnlockBits(位图数据);
        }
        if (显示网格) {
            Pen 网格笔 = new Pen(Color.Gray);
            float[] 虚线样式 = new float[] { 1f, 2f };
            网格笔.DashPattern = 虚线样式;
            for (int y = 原图局部框.Top; y < 原图局部框.Bottom; y++)
                g.DrawLine(网格笔, 0, (y - 原图局部框.Top) * 缩放比例 - 1, 原图局部框.Width * 缩放比例 - 2, (y - 原图局部框.Top) * 缩放比例 - 1);
            for (int x = 原图局部框.Left; x < 原图局部框.Right; x++)
                g.DrawLine(网格笔, (x - 原图局部框.Left) * 缩放比例 - 1, 0, (x - 原图局部框.Left) * 缩放比例 - 1, 原图局部框.Height * 缩放比例 - 2);
        }
        
        Rectangle 缩略局部框 = new ((int)(原图局部框.X * 缩略比), (int)(原图局部框.Y * 缩略比), (int)(原图局部框.Width * 缩略比 + 1), (int)(原图局部框.Height * 缩略比 + 1));
        using Graphics g缩略 = 缩略图框.CreateGraphics();
        缩略图框.Refresh();
        g缩略.DrawRectangle(new Pen(Color.Red), 缩略局部框);
        g缩略.DrawRectangle(new Pen(Color.Cyan), 缩略局部框.X - 1, 缩略局部框.Y - 1, 缩略局部框.Width + 2, 缩略局部框.Height + 2);

    }//只把显示区域能装下的原图局部拿出来放大显示了.

    private void 获取局部中心点坐标() {
        int 右 = Math.Min(Size.Width, 图数据.位图.Width) ;
        int 下 = Math.Min(Size.Height, 图数据.位图.Height);//图片比显示区域小时,右下坐标取图片的右下点.
        局部中心 = new(右 / 2 + 1 - 图片框.Location.X, 下 / 2 + 1 - 图片框.Location.Y);
    }//没有点右键指定放大哪里时,如果图片小,没有出现滚动条,就把图片中心作为放大区域的中心;如果图片较大,就把显示区域的中心作为放大中心.

    public void 刷新(int dx = 0,int dy = 0) {
        if (缩放比例 > 1) {
            局部中心.X += dx;
            局部中心.Y += dy;
            刷新局部放大图();
        }
        图片框.Refresh();
    }

    #endregion

    public void 关闭() {        
        Dispose();
        GC.Collect();
    }
}

