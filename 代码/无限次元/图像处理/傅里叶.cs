using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 图像处理;

public class 傅里叶 {
    private const int 红 = 2, 绿 = 1, 蓝 = 0; //Bitmap位图数据中,蓝色Blue的值存在低地址上

    public delegate 复数 W委托(int N, int k, int n);
    static public 复数[] DFT(复数[] 待变序列) {
        int 序列长度N = 待变序列.Length;
        复数[] DFT结果 = new 复数[序列长度N];
        for (int k = 0; k <= 序列长度N / 2; k++) {
            DFT结果[k] = new 复数(0, 0);
            for (int n = 0; n < 序列长度N; n++)
                DFT结果[k] = DFT结果[k] + 待变序列[n] * W(序列长度N, k, n);
        }
        for (int k = 序列长度N / 2 + 1; k < 序列长度N; k++) //系数k和系数N-k是共轭的
            DFT结果[k] = DFT结果[序列长度N - k].Conjugate();
        return DFT结果;
    }

    static public byte[] 字节IDFT(复数[] DFT结果) {
        int 序列长度N = DFT结果.Length;
        复数[] 原序列 = new 复数[序列长度N];
        byte[] 原字节序列 = new byte[序列长度N];
        for (int n = 0; n < 序列长度N; n++) {
            原序列[n] = new 复数(0, 0);
            for (int k = 0; k < 序列长度N; k++)
                原序列[n] = 原序列[n] + DFT结果[k] * W(序列长度N, -k, n);
            原字节序列[n] = (byte)(原序列[n].Real / 序列长度N + 0.5);
        }
        return 原字节序列;
    }

    static public 复数[] IDFT(复数[] DFT结果, W委托 W托) {
        int 序列长度N = DFT结果.Length;
        复数[] 原序列 = new 复数[序列长度N];
        for (int n = 0; n < 序列长度N; n++) {
            原序列[n] = new 复数(0, 0);
            for (int k = 0; k < 序列长度N; k++)
                原序列[n] = 原序列[n] + DFT结果[k] * W托(序列长度N, -k, n);
            原序列[n] = 原序列[n] / 序列长度N;
        }
        return 原序列;
    }

    static 复数 W(int N, int k, int n) {
        double 转角 = 2 * Math.PI * k * n / N;
        return new 复数(Math.Cos(转角), -Math.Sin(转角));
    }

    static public 复数[] DFT(byte[] 字节待变序列) {
        复数[] 待变序列 = new 复数[字节待变序列.Length];
        for (int i = 0; i < 字节待变序列.Length; i++)
            待变序列[i] = new 复数(字节待变序列[i], 0);
        return DFT(待变序列);
    }

    static public 复数[] FFT(复数[] sourceData, int countN) {
        int r = Convert.ToInt32(Math.Log(countN, 2));//求fft的级数
                                                     //求加权系数W
        复数[] w = new 复数[countN / 2];
        复数[] interVar1 = new 复数[countN];
        复数[] interVar2 = new 复数[countN];

        interVar1 = (复数[])sourceData.Clone();

        for (int i = 0; i < countN / 2; i++) {
            double angle = -i * Math.PI * 2 / countN;
            w[i] = new 复数(Math.Cos(angle), Math.Sin(angle));
        }

        //核心部分
        for (int i = 0; i < r; i++) {
            int interval = 1 << i;
            int halfN = 1 << (r - i);
            for (int j = 0; j < interval; j++) {
                int gap = j * halfN;
                for (int k = 0; k < halfN / 2; k++) {
                    interVar2[k + gap] = interVar1[k + gap] + interVar1[k + gap + halfN / 2];
                    interVar2[k + halfN / 2 + gap] = (interVar1[k + gap] - interVar1[k + gap + halfN / 2]) * w[k * interval];
                }
            }
            interVar1 = (复数[])interVar2.Clone();
        }
        for (uint j = 0; j < countN; j++) {
            uint rev = 0;
            uint num = j;
            for (int i = 0; i < r; i++) {
                rev <<= 1;
                rev |= num & 1;
                num >>= 1;
            }
            interVar2[rev] = interVar1[j];
        }
        return interVar2;
    }

    static public 复数[] FFT(byte[] 字节待变序列) {
        复数[] 待变序列 = new 复数[字节待变序列.Length];
        for (int i = 0; i < 字节待变序列.Length; i++)
            待变序列[i] = new 复数(字节待变序列[i], 0);
        return FFT(待变序列, 待变序列.Length);
    }

    static public 复数[] 递归FFT(byte[] 字节待变序列) {
        复数[] 待变序列 = new 复数[字节待变序列.Length];
        for (int i = 0; i < 字节待变序列.Length; i++)
            待变序列[i] = new 复数(字节待变序列[i], 0);
        return 递归FFT(待变序列);
    }

    static public 复数[] 递归FFT(复数[] 待变序列) {
        int 序列长度N = 待变序列.Length;
        if (序列长度N == 1)
            return 待变序列;
        else {
            复数[] DFT结果 = new 复数[序列长度N];
            复数[] 偶序列 = new 复数[序列长度N / 2];
            复数[] 奇序列 = new 复数[序列长度N / 2];
            for (int n = 0; n < 序列长度N / 2; n++) {
                偶序列[n] = 待变序列[n * 2];
                奇序列[n] = 待变序列[n * 2 + 1];
            }
            偶序列 = 递归FFT(偶序列);
            奇序列 = 递归FFT(奇序列);
            for (int k = 0; k < 序列长度N / 2; k++) {
                复数 奇xW = 奇序列[k] * W(序列长度N, k, 1);
                DFT结果[k] = 偶序列[k] + 奇xW;
                DFT结果[k + 序列长度N / 2] = 偶序列[k] - 奇xW;
            }
            return DFT结果;
        }
    }

 

   

  
    



}
