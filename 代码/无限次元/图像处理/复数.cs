using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 图像处理;

public class 复数 {//本代码源自网络
    private double real = 0.0;
    private double imaginary = 0.0;

    public double Real {
        get {
            return real;
        }
        set {
            real = value;
        }
    }


    public double Imaginary {
        get {
            return imaginary;
        }
        set {
            imaginary = value;
        }
    }

    public 复数() {
    }

    public 复数(double dbreal, double dbimag) {
        real = dbreal;
        imaginary = dbimag;
    }

    public 复数(复数 other) {
        real = other.real;
        imaginary = other.imaginary;
    }

    public static 复数 operator +(复数 comp1, 复数 comp2) {
        return comp1.Add(comp2);
    }

    public static 复数 operator -(复数 comp1, 复数 comp2) {
        return comp1.Subtract(comp2);
    }

    public static 复数 operator *(复数 comp1, 复数 comp2) {
        return comp1.Multiply(comp2);
    }

    public static 复数 operator /(复数 comp1, double real2) {
        double x = comp1.real / real2;
        double y = comp1.imaginary / real2;
        return new 复数(x, y);
    }

    public 复数 Add(复数 comp) {
        double x = real + comp.real;
        double y = imaginary + comp.imaginary;
        return new 复数(x, y);
    }

    public 复数 Subtract(复数 comp) {
        double x = real - comp.real;
        double y = imaginary - comp.imaginary;
        return new 复数(x, y);
    }

    public 复数 Multiply(复数 comp) {
        double x = real * comp.real - imaginary * comp.imaginary;
        double y = real * comp.imaginary + imaginary * comp.real;
        return new 复数(x, y);
    }

    public double Abs() {
        double x = Math.Abs(real);
        double y = Math.Abs(imaginary);

        if (real == 0) {
            return y;
        }
        if (imaginary == 0) {
            return x;
        }

        if (x > y) {
            return (x * Math.Sqrt(1 + (y / x) * (y / x)));
        }
        else {
            return (y * Math.Sqrt(1 + (x / y) * (x / y)));
        }
    }

    public double Angle() {
        if (real == 0 && imaginary == 0)
            return 0;
        if (real == 0)
            if (imaginary > 0)
                return Math.PI / 2;
            else
                return -Math.PI / 2;
        else if (imaginary == 0)
            if (real > 0)
                return 0;
            else
                return Math.PI;
        else
            return Math.Atan2(imaginary, real);

    }

    public 复数 Conjugate() {
        return new 复数(this.real, -this.imaginary);
    }

    public override string ToString() {
        if (imaginary > 0)
            return real.ToString("0.0000") + " + " + imaginary.ToString("0.0000") + "i";
        else if (imaginary == 0)
            return real.ToString("0.0000");
        else
            return real.ToString("0.0000") + " - " + Math.Abs(imaginary).ToString("0.0000") + "i";
    }

    public Point ToPoint(Point 原点) {
        return new Point((int)(原点.X + real + 0.5), (int)(原点.Y + imaginary + 0.5));
    }
}


