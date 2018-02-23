namespace ThemeBrowser
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using ThemeBrowser.Extensions;
    using ThemeBrowser.Native;
    using ThemeCore.Native;

    public class PreviewImageDialogViewModel : ViewModel
    {
        private string message;
        private readonly Bitmap sourceBitmap;
        private readonly SafeBitmapHandle sourceBitmapGdi;
        private ImageSource imageSource;
        private double imageWidth;
        private double imageHeight;

        private Int32Rect sourceRect;
        private Int32Rect destRect;
        private Int32Rect sizingMargins;
        private Int32Rect? clippingRect;
        private int drawOption;

        public PreviewImageDialogViewModel(Bitmap sourceBitmap)
        {
            this.sourceBitmap = sourceBitmap;
            sourceBitmapGdi = sourceBitmap.GetHbitmapHandle();

            imageWidth = 100;
            imageHeight = 100;
            sourceRect = new Int32Rect(0, 0, sourceBitmap.Width, sourceBitmap.Height / 6);
            destRect = new Int32Rect(0, 0, (int)ImageWidth, (int)ImageHeight);
            sizingMargins = new Int32Rect(6, 5, 6, 5);
            drawOption = 4;

            RenderImage();
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public Int32Rect SourceRect
        {
            get => sourceRect;
            set
            {
                if (SetProperty(ref sourceRect, value))
                    RenderImage();
            }
        }

        public Int32Rect DestRect
        {
            get => destRect;
            set
            {
                if (SetProperty(ref destRect, value))
                    RenderImage();
            }
        }

        public Int32Rect SizingMargins
        {
            get => sizingMargins;
            set
            {
                if (SetProperty(ref sizingMargins, value))
                    RenderImage();
            }
        }

        public Int32Rect? ClippingRect
        {
            get => clippingRect;
            set
            {
                if (SetProperty(ref clippingRect, value))
                    RenderImage();
            }
        }

        public int DrawOption
        {
            get => drawOption;
            set
            {
                if (SetProperty(ref drawOption, value))
                    RenderImage();
            }
        }

        public ImageSource ImageSource
        {
            get => imageSource;
            set => SetProperty(ref imageSource, value);
        }

        public double ImageWidth
        {
            get => imageWidth;
            set
            {
                if (SetProperty(ref imageWidth, Math.Max(value, 10)))
                    RenderImage();
            }
        }

        public double ImageHeight
        {
            get => imageHeight;
            set
            {
                if (SetProperty(ref imageHeight, Math.Max(value, 10)))
                    RenderImage();
            }
        }

        private void RenderImage()
        {
            Message = null;

            var width = (int)ImageWidth;
            var height = (int)ImageHeight;
            if (width == 0 || height == 0)
                return;

            var srcRect = Convert(SourceRect);
            var dstRect = Convert(DestRect);
            dstRect = new RECT(0, 0, width, height);
            var clipRect = ClippingRect != null ? Convert(ClippingRect.Value) : dstRect;
            var margins = new MARGINS {
                cxLeftWidth = SizingMargins.X,
                cyTopHeight = SizingMargins.Y,
                cxRightWidth = SizingMargins.Width,
                cyBottomHeight = SizingMargins.Height
            };

            uint transparentColor = 0x000000;

            using (var bitmap = new Bitmap(width, height)) {
                using (var gfx = Graphics.FromImage(bitmap)) {
                    var hdc = gfx.GetHdc();

                    var gds = new GDIDRAWSTREAM();
                    gds.signature = 0x44727753;
                    gds.hDC = (uint)hdc.ToInt64();
                    gds.hImage = (uint)sourceBitmapGdi.DangerousGetHandle().ToInt64();
                    gds.one = 1;
                    gds.nine = 9;
                    gds.rcSrc = srcRect;
                    gds.rcClip = clipRect;
                    gds.rcDest = dstRect;
                    gds.drawOption = (uint)DrawOption;
                    gds.leftArcValue = (uint)margins.cxLeftWidth;
                    gds.rightArcValue = (uint)margins.cxRightWidth;
                    gds.topArcValue = (uint)margins.cyTopHeight;
                    gds.bottomArcValue = (uint)margins.cyBottomHeight;
                    gds.crTransparent = transparentColor;

                    var success = GdiNativeMethods.GdiDrawStream(
                        hdc, (uint)Marshal.SizeOf<GDIDRAWSTREAM>(), ref gds);

                    gfx.ReleaseHdc(hdc);

                    if (!success) {
                        var ec = Marshal.GetLastWin32Error();
                        Message = string.Format("Draw failed: ec={0} ({1})", ec, new Win32Exception(ec).Message);
                        return;
                    }
                }

                ImageSource = ImagingUtils.ConvertBitmap(bitmap).EnsureFrozen();
            }
        }

        private static RECT Convert(Int32Rect rect)
        {
            return new RECT(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
