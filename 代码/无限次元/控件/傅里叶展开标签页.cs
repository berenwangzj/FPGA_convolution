using System.Drawing.Imaging;

namespace 控件;
public class 傅里叶展开标签页 : TabPage, I标签页 {
    const int 图宽 = 1892;
    const int 图高 = 1600;
    const int 横间隙 = 4;
    const int 纵间隙 = 4;
    int 波形图高 = 128;
    int 波形图宽 = 512;
    int 每行几个方框 = 3;
    int 方框数量 = 36;
    const int 数据点间隔 = 512 / 64;
    int 分量数据长度 = 128;
    static Color 背景色 = Color.LightGray;
    public PictureBox 图片框;
    Bitmap 展开图 = new (图宽, 图高, PixelFormat.Format24bppRgb);
    Graphics g;
    Point 当前点坐标, 按下时框内坐标, 上一坐标;
    bool 左键按下 = false;
    bool 画曲线 = false;
    bool 拖分量 = false;
    Rectangle[] 方框数组;
    bool[] 框被选中;
    int 当前框号 = -1;
    public byte[] 展开数据;
    byte[][] 频率分量字节数据 = new byte[32][];
    double[][] 频率分量数据 = new double[32][];
    double[] 分量幅度 = new double[32];
    double[] 分量相位 = new double[32];
    bool 已展开 = false;
    复数[] 展开结果;
    static public 局部放大图 局部放大控件;

    public 傅里叶展开标签页(ContextMenuStrip 右键菜单, byte[] 展开数据, string Tab名称 = "傅里叶") {
        if (展开数据.Length != 64) { MessageBox.Show("数据长度必须是64!"); return; }
        this.展开数据 = 展开数据;
        g = Graphics.FromImage(展开图);
        g.Clear(背景色);
        图片框 = new PictureBox {
            SizeMode = PictureBoxSizeMode.AutoSize,
            Image = 展开图,
            ContextMenuStrip = 右键菜单
        };
        AutoScroll = true;
        Controls.Add(图片框);
        Text = Tab名称 + "展开";
        图片框.MouseMove += 图片框_MouseMove;
        图片框.MouseDown += 图片框_MouseDown;
        图片框.MouseUp += 图片框_MouseUp;
        图片框.MouseClick += 图片框_MouseClick;
        方框数组 = new Rectangle[方框数量];
        框被选中 = new bool[方框数量];

        一些初始化();
        写提示();
    }

    public void 展开() {
        清空选择();
        展开结果 = 傅里叶.FFT(展开数据);
        擦除框(0);
        绘图.画字节数据(展开数据, g, 方框数组[0], Color.Black);
        

        for (int k = 0; k < 32; k++) {
            double 幅度 = 4 * 展开结果[k].Abs() / 展开数据.Length / byte.MaxValue;
            if (k == 0)
                幅度 = 展开结果[0].Real / 展开结果.Length / byte.MaxValue;
            if (幅度 > 1) 幅度 = 1;
            double 相位 = 展开结果[k].Angle(); //这个角度是Cos的角度,所以要加π/2变成Sin的角度,因为下面的绘图函数画的是Sin图像
            if (k > 0) {
                绘图.生成正弦数据(频率分量字节数据[k - 1], k, 幅度, 相位 + Math.PI / 2);
                绘图.生成正弦数据(频率分量数据[k - 1], k, 幅度, 相位 + Math.PI / 2);
                擦除框(2 + k);
                绘图.画字节数据(频率分量字节数据[k - 1], g, 方框数组[2 + k], Color.Black);
              
                框中写字(k.ToString(), 2 + k);
            }
            分量幅度[k] = 幅度;
            分量相位[k] = 相位;
        }
        画谱();
        已展开 = true;
        图片框.Refresh();
    }

    private void 写提示() {
        框中写字("按住左键滑动可修改此曲线", 0);
        框中写字("右键菜单中可点'展开'", 0, new Point(0, 18));
        框中写字("按住Shift拖鼠标可修改分量的幅度和相位", 3, new Point(0, 18));
        框中写字("单击每个分量可看到逐步叠加效果", 3);
        框中写字("左右拖改相位,上下拖改幅度", 3, new Point(0, 36));
    }
    private void 一些初始化() {
        for (int i = 0; i < 方框数组.Length; i++) {
            int x = i % 每行几个方框;
            int y = i / 每行几个方框;
            方框数组[i] = new Rectangle((x + 1) * 横间隙 + x * 波形图宽, (y + 1) * 纵间隙 + y * 波形图高, 波形图宽, 波形图高);
        }
        for (int i = 0; i < 频率分量字节数据.GetLength(0); i++) {
            频率分量字节数据[i] = new byte[分量数据长度];
            频率分量数据[i] = new double[分量数据长度];
        }
        绘图.画字节数据(展开数据, g, 方框数组[0], Color.Black);
       
    }

    private void 画谱() {
        Rectangle 幅度谱框 = new Rectangle(1570, 20, 256, 128);
        Rectangle 相位谱框 = new Rectangle(1570, 60 + 128, 256, 128);
        Rectangle 擦除框 = new Rectangle(1569, 0, 260, 400);
        g.FillRectangle(new SolidBrush(背景色), 擦除框);
        绘图.画柱状图(g, 分量幅度, 1, 0, 幅度谱框, "幅度谱", Color.Black);
        绘图.画柱状图(g, 分量相位, Math.PI * 2, 0, 相位谱框, "相位谱", Color.Black);
    }

    private void 画分量叠加图() {
        double[] 分量和 = new double[分量数据长度];
        byte[] 字节分量和 = new byte[分量数据长度];
        擦除框(1);
        擦除框(2);
        绘图.画字节数据(展开数据, g, 方框数组[1], Color.Gray);
        
        for (int i = 0; i < 32; i++) {
            if (框被选中[i + 3]) {
                for (int n = 0; n < 分量和.Length; n++)
                    分量和[n] += 频率分量数据[i][n];
                绘图.画字节数据(频率分量字节数据[i], g, 方框数组[1], 图像.色谱颜色24[(i * 2 + i / 12) % 24]);
                
            }

        }
        for (int n = 0; n < 分量和.Length; n++) {
            int Data = (int)((分量和[n]) / 2 * 255 + 0.5 + 展开结果[0].Real / 展开结果.Length);
            if (Data > 255)
                字节分量和[n] = 255;
            else if (Data < 0)
                字节分量和[n] = 0;
            else
                字节分量和[n] = (byte)Data;
        }

        绘图.画字节数据(字节分量和, g, 方框数组[2], Color.Green);
        
    }


    private int 鼠标在哪个框(Point 鼠标位置, out Point 框内坐标) {
        框内坐标 = new Point();
        int 框号 = -1;
        int x = 鼠标位置.X / (波形图宽 + 横间隙);
        int y = 鼠标位置.Y / (波形图高 + 纵间隙);
        int 临时框号 = y * 每行几个方框 + x;
        if (临时框号 >= 0 && 临时框号 <= 方框数量)
            if (鼠标位置.X >= 方框数组[临时框号].X && 鼠标位置.Y >= 方框数组[临时框号].Y) {
                框号 = 临时框号;
                框内坐标.X = 鼠标位置.X - 方框数组[临时框号].X;
                框内坐标.Y = 方框数组[临时框号].Height - (鼠标位置.Y - 方框数组[临时框号].Y);
            }
        return 框号;
    }

    private void 图片框_MouseClick(object sender, MouseEventArgs e) {
        当前点坐标 = 图片框.PointToClient(Cursor.Position);
        Focus(); //单击时让Tabpage获得焦点好让滚轮运作
        if (e.Button == MouseButtons.Left && 已展开 && !((Control.ModifierKeys & Keys.Shift) == Keys.Shift)) {
            当前框号 = 鼠标在哪个框(当前点坐标, out 按下时框内坐标);
            if (当前框号 > 2) {
                if (框被选中[当前框号]) {
                    g.DrawRectangle(new Pen(背景色), 方框数组[当前框号]);
                    框被选中[当前框号] = false;
                }
                else {
                    g.DrawRectangle(new Pen(Color.Green), 方框数组[当前框号]);
                    框被选中[当前框号] = true;
                }
                画分量叠加图();
                图片框.Refresh();
            }
        }

    }

    private void 图片框_MouseUp(object sender, MouseEventArgs e) {
        左键按下 = false;
        画曲线 = false;
        拖分量 = false;
        Cursor.Clip = Rectangle.Empty;
    }

    private void 图片框_MouseDown(object sender, MouseEventArgs e) {
        当前点坐标 = 图片框.PointToClient(Cursor.Position);
        if (e.Button == MouseButtons.Left) 左键按下 = true;
        if (左键按下) {
            当前框号 = 鼠标在哪个框(当前点坐标, out 按下时框内坐标);
            上一坐标 = 按下时框内坐标;
            if (当前框号 == 0 ) {
                画曲线 = true;
                Cursor.Clip = RectangleToScreen(方框数组[0]);
            }
            if (当前框号 >= 3 && 当前框号 < 3 + 31 && (Control.ModifierKeys & Keys.Shift) == Keys.Shift) {
                拖分量 = true;
            }
        }
    }

    private void 图片框_MouseMove(object sender, MouseEventArgs e) {
        当前点坐标 = 图片框.PointToClient(Cursor.Position);

        if (!左键按下) {
            局部放大控件.显示像素信息(展开图,当前点坐标);
        }
        else {
            鼠标在哪个框(当前点坐标, out Point 当前框内坐标);
            if (画曲线) {

                擦除框(0);
                展开数据[当前框内坐标.X / 数据点间隔] = (byte)(当前框内坐标.Y * 2);
                绘图.画字节数据(展开数据, g, 方框数组[0], Color.Black);
                图片框.Refresh();
            }
            if (拖分量) {
                int dx = 当前框内坐标.X - 上一坐标.X;
                int dy = 当前框内坐标.Y - 上一坐标.Y;
                int k = 当前框号 - 3 + 1;
                if (Math.Abs(dy) > Math.Abs(dx)) {
                    分量幅度[k] = 分量幅度[k] + (double)dy / 64;//Math.Abs(当前框内坐标.Y - 128) / (double)128;
                    if (分量幅度[k] > 1)
                        分量幅度[k] = 1;
                    if (分量幅度[k] < 0)
                        分量幅度[k] = 0;
                }
                else {
                    分量相位[k] = 分量相位[k] + (double)-dx / 64 * Math.PI;
                    if (分量相位[k] > Math.PI * 2)
                        分量相位[k] = 分量相位[k] % (Math.PI * 2);
                    if (分量相位[k] < 0)
                        分量相位[k] = (Math.PI * 2) + 分量相位[k] % (Math.PI * 2);
                }
                绘图.生成正弦数据(频率分量字节数据[k - 1], k, 分量幅度[k], 分量相位[k] + Math.PI / 2);
                绘图.生成正弦数据(频率分量数据[k - 1], k, 分量幅度[k], 分量相位[k] + Math.PI / 2);
                擦除框(k + 2);
                绘图.画字节数据(频率分量字节数据[k - 1], g, 方框数组[k + 2], Color.Black);
                
                if (框被选中[当前框号])
                    g.DrawRectangle(new Pen(Color.Green), 方框数组[当前框号]);
                画分量叠加图();
                画谱();
                图片框.Refresh();
                上一坐标 = 当前框内坐标;
            }
        }


    }

    public void 关闭() {
        g.Dispose();
        展开图.Dispose();
        图片框.Dispose();
        Dispose();
        GC.Collect();
    }

    public void 清空选择() {
        if (已展开) {
            for (int i = 0 + 3; i < 32 + 3; i++) {
                if (框被选中[i]) {
                    框被选中[i] = false;
                    g.DrawRectangle(new Pen(背景色), 方框数组[i]);
                }
            }
            擦除框(1);
            擦除框(2);
            图片框.Refresh();
        }
    }

    private void 擦除框(int 框号) {
        g.FillRectangle(new SolidBrush(背景色), 方框数组[框号]);
        g.DrawRectangle(new Pen(背景色), 方框数组[框号]);
    }

    private void 框中写字(string 文本, int 框号, Point 位置 = new Point()) {
        g.DrawString(文本, 绘图.TimesNR字体(12), new SolidBrush(Color.Purple), new Point(位置.X + 方框数组[框号].X, 位置.Y + 方框数组[框号].Y));
    }
}

