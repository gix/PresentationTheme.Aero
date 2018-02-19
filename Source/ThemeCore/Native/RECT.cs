namespace ThemeCore.Native
{
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(Rectangle r)
        {
            left = r.X;
            top = r.Y;
            right = r.Right;
            bottom = r.Bottom;
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public override string ToString()
        {
            return string.Concat("Left=", left, ", Top=", top, ", Right=", right, ", Bottom=", bottom);
        }

        public void SetRect(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public class CRECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public CRECT()
        {
        }

        public CRECT(Rectangle r)
        {
            left = r.X;
            top = r.Y;
            right = r.Right;
            bottom = r.Bottom;
        }

        public CRECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public override string ToString()
        {
            return string.Concat("Left=", left, ", Top=", top, ", Right=", right, ", Bottom=", bottom);
        }
    }
}
