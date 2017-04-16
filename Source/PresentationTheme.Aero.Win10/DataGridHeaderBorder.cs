namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public sealed class DataGridHeaderBorder : Border
    {
        public static readonly DependencyProperty IsHoveredProperty = DependencyProperty.Register("IsHovered", typeof(bool), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)false, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register("IsPressed", typeof(bool), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register("IsClickable", typeof(bool), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register("SortDirection", typeof(ListSortDirection?), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)false, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register("SeparatorBrush", typeof(Brush), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((PropertyChangedCallback)null));
        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register("SeparatorVisibility", typeof(Visibility), typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)Visibility.Visible));
        private static object _cacheAccess = new object();
        private static List<Freezable> _freezableCache;

        public bool IsHovered
        {
            get => (bool)this.GetValue(DataGridHeaderBorder.IsHoveredProperty);
            set => this.SetValue(DataGridHeaderBorder.IsHoveredProperty, value);
        }

        public bool IsPressed
        {
            get => (bool)this.GetValue(DataGridHeaderBorder.IsPressedProperty);
            set => this.SetValue(DataGridHeaderBorder.IsPressedProperty, value);
        }

        public bool IsClickable
        {
            get => (bool)this.GetValue(DataGridHeaderBorder.IsClickableProperty);
            set => this.SetValue(DataGridHeaderBorder.IsClickableProperty, value);
        }

        public ListSortDirection? SortDirection
        {
            get => (ListSortDirection?)this.GetValue(DataGridHeaderBorder.SortDirectionProperty);
            set => this.SetValue(DataGridHeaderBorder.SortDirectionProperty, (object)value);
        }

        public bool IsSelected
        {
            get => (bool)this.GetValue(DataGridHeaderBorder.IsSelectedProperty);
            set => this.SetValue(DataGridHeaderBorder.IsSelectedProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(DataGridHeaderBorder.OrientationProperty);
            set => this.SetValue(DataGridHeaderBorder.OrientationProperty, (object)value);
        }

        private bool UsingBorderImplementation
        {
            get
            {
                if (this.Background == null)
                    return this.BorderBrush != null;
                return true;
            }
        }

        public Brush SeparatorBrush
        {
            get => (Brush)this.GetValue(DataGridHeaderBorder.SeparatorBrushProperty);
            set => this.SetValue(DataGridHeaderBorder.SeparatorBrushProperty, (object)value);
        }

        public Visibility SeparatorVisibility
        {
            get => (Visibility)this.GetValue(DataGridHeaderBorder.SeparatorVisibilityProperty);
            set => this.SetValue(DataGridHeaderBorder.SeparatorVisibilityProperty, (object)value);
        }

        private Thickness DefaultPadding
        {
            get
            {
                Thickness thickness = new Thickness(3.0);
                Thickness? themeDefaultPadding = this.ThemeDefaultPadding;
                if (!themeDefaultPadding.HasValue) {
                    if (this.Orientation == Orientation.Vertical)
                        thickness.Right = 15.0;
                } else
                    thickness = themeDefaultPadding.Value;
                if (this.IsPressed && this.IsClickable) {
                    ++thickness.Left;
                    ++thickness.Top;
                    --thickness.Right;
                    --thickness.Bottom;
                }
                return thickness;
            }
        }

        private Thickness? ThemeDefaultPadding
        {
            get
            {
                if (this.Orientation == Orientation.Vertical)
                    return new Thickness?(new Thickness(5.0, 4.0, 5.0, 4.0));
                return new Thickness?();
            }
        }

        static DataGridHeaderBorder()
        {
            UIElement.SnapsToDevicePixelsProperty.OverrideMetadata(typeof(DataGridHeaderBorder), (PropertyMetadata)new FrameworkPropertyMetadata((object)true));
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (this.UsingBorderImplementation)
                return base.MeasureOverride(constraint);
            UIElement child = this.Child;
            if (child == null)
                return new Size();
            Thickness thickness = this.Padding;
            if (thickness.Equals(new Thickness()))
                thickness = this.DefaultPadding;
            double num1 = constraint.Width;
            double num2 = constraint.Height;
            if (!double.IsInfinity(num1))
                num1 = Math.Max(0.0, num1 - thickness.Left - thickness.Right);
            if (!double.IsInfinity(num2))
                num2 = Math.Max(0.0, num2 - thickness.Top - thickness.Bottom);
            child.Measure(new Size(num1, num2));
            Size desiredSize = child.DesiredSize;
            return new Size(desiredSize.Width + thickness.Left + thickness.Right, desiredSize.Height + thickness.Top + thickness.Bottom);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.UsingBorderImplementation)
                return base.ArrangeOverride(arrangeSize);
            UIElement child = this.Child;
            if (child != null) {
                Thickness thickness = this.Padding;
                if (thickness.Equals(new Thickness()))
                    thickness = this.DefaultPadding;
                double width = Math.Max(0.0, arrangeSize.Width - thickness.Left - thickness.Right);
                double height = Math.Max(0.0, arrangeSize.Height - thickness.Top - thickness.Bottom);
                child.Arrange(new Rect(thickness.Left, thickness.Top, width, height));
            }
            return arrangeSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (this.UsingBorderImplementation)
                base.OnRender(dc);
            else
                this.RenderTheme(dc);
        }

        private static double Max0(double d)
        {
            return Math.Max(0.0, d);
        }

        private static void EnsureCache(int size)
        {
            if (DataGridHeaderBorder._freezableCache != null)
                return;
            lock (DataGridHeaderBorder._cacheAccess) {
                if (DataGridHeaderBorder._freezableCache != null)
                    return;
                DataGridHeaderBorder._freezableCache = new List<Freezable>(size);
                for (int local_2 = 0; local_2 < size; ++local_2)
                    DataGridHeaderBorder._freezableCache.Add((Freezable)null);
            }
        }

        private static void ReleaseCache()
        {
            if (DataGridHeaderBorder._freezableCache == null)
                return;
            lock (DataGridHeaderBorder._cacheAccess)
                DataGridHeaderBorder._freezableCache = (List<Freezable>)null;
        }

        private static Freezable GetCachedFreezable(int index)
        {
            lock (DataGridHeaderBorder._cacheAccess)
                return DataGridHeaderBorder._freezableCache[index];
        }

        private static void CacheFreezable(Freezable freezable, int index)
        {
            lock (DataGridHeaderBorder._cacheAccess) {
                if (DataGridHeaderBorder._freezableCache[index] == null)
                    return;
                DataGridHeaderBorder._freezableCache[index] = freezable;
            }
        }

        private void RenderTheme(DrawingContext dc)
        {
            Size renderSize = this.RenderSize;
            bool flag1 = this.Orientation == Orientation.Horizontal;
            bool flag2 = this.IsClickable && this.IsEnabled;
            bool flag3 = flag2 && this.IsHovered;
            bool flag4 = flag2 && this.IsPressed;
            ListSortDirection? sortDirection = this.SortDirection;
            bool hasValue = sortDirection.HasValue;
            bool isSelected = this.IsSelected;
            bool flag5 = !flag3 && !flag4 && !hasValue && !isSelected;
            DataGridHeaderBorder.EnsureCache(19);
            if (flag1) {
                Matrix matrix1 = new Matrix();
                matrix1.RotateAt(-90.0, 0.0, 0.0);
                Matrix matrix2 = new Matrix();
                matrix2.Translate(0.0, renderSize.Height);
                MatrixTransform matrixTransform = new MatrixTransform(matrix1 * matrix2);
                matrixTransform.Freeze();
                dc.PushTransform((Transform)matrixTransform);
                double width = renderSize.Width;
                renderSize.Width = renderSize.Height;
                renderSize.Height = width;
            }
            if (flag5) {
                LinearGradientBrush linearGradientBrush = (LinearGradientBrush)DataGridHeaderBorder.GetCachedFreezable(0);
                if (linearGradientBrush == null) {
                    linearGradientBrush = new LinearGradientBrush();
                    linearGradientBrush.StartPoint = new Point();
                    linearGradientBrush.EndPoint = new Point(0.0, 1.0);
                    linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 0.0));
                    linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 0.4));
                    linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)252, (byte)252, (byte)253), 0.4));
                    linearGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)251, (byte)252, (byte)252), 1.0));
                    linearGradientBrush.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)linearGradientBrush, 0);
                }
                dc.DrawRectangle((Brush)linearGradientBrush, (Pen)null, new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));
            }
            DataGridHeaderBorder.AeroFreezables aeroFreezables1 = DataGridHeaderBorder.AeroFreezables.NormalBackground;
            if (flag4)
                aeroFreezables1 = DataGridHeaderBorder.AeroFreezables.PressedBackground;
            else if (flag3)
                aeroFreezables1 = DataGridHeaderBorder.AeroFreezables.HoveredBackground;
            else if (hasValue | isSelected)
                aeroFreezables1 = DataGridHeaderBorder.AeroFreezables.SortedBackground;
            LinearGradientBrush linearGradientBrush1 = (LinearGradientBrush)DataGridHeaderBorder.GetCachedFreezable((int)aeroFreezables1);
            if (linearGradientBrush1 == null) {
                linearGradientBrush1 = new LinearGradientBrush();
                linearGradientBrush1.StartPoint = new Point();
                linearGradientBrush1.EndPoint = new Point(0.0, 1.0);
                switch (aeroFreezables1) {
                    case DataGridHeaderBorder.AeroFreezables.NormalBackground:
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 0.0));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)247, (byte)248, (byte)250), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)241, (byte)242, (byte)244), 1.0));
                        break;
                    case DataGridHeaderBorder.AeroFreezables.PressedBackground:
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)188, (byte)228, (byte)249), 0.0));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)188, (byte)228, (byte)249), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)141, (byte)214, (byte)247), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)138, (byte)209, (byte)245), 1.0));
                        break;
                    case DataGridHeaderBorder.AeroFreezables.HoveredBackground:
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)227, (byte)247, byte.MaxValue), 0.0));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)227, (byte)247, byte.MaxValue), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)189, (byte)237, byte.MaxValue), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)183, (byte)231, (byte)251), 1.0));
                        break;
                    case DataGridHeaderBorder.AeroFreezables.SortedBackground:
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)242, (byte)249, (byte)252), 0.0));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)242, (byte)249, (byte)252), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)225, (byte)241, (byte)249), 0.4));
                        linearGradientBrush1.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)216, (byte)236, (byte)246), 1.0));
                        break;
                }
                linearGradientBrush1.Freeze();
                DataGridHeaderBorder.CacheFreezable((Freezable)linearGradientBrush1, (int)aeroFreezables1);
            }
            dc.DrawRectangle((Brush)linearGradientBrush1, (Pen)null, new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));
            if (renderSize.Width >= 2.0) {
                DataGridHeaderBorder.AeroFreezables aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.NormalSides;
                if (flag4)
                    aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.PressedSides;
                else if (flag3)
                    aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.HoveredSides;
                else if (hasValue | isSelected)
                    aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.SortedSides;
                if (this.SeparatorVisibility == Visibility.Visible) {
                    Brush brush;
                    if (this.SeparatorBrush != null) {
                        brush = this.SeparatorBrush;
                    } else {
                        brush = (Brush)DataGridHeaderBorder.GetCachedFreezable((int)aeroFreezables2);
                        if (brush == null) {
                            LinearGradientBrush linearGradientBrush2 = (LinearGradientBrush)null;
                            if (aeroFreezables2 != DataGridHeaderBorder.AeroFreezables.SortedSides) {
                                linearGradientBrush2 = new LinearGradientBrush();
                                linearGradientBrush2.StartPoint = new Point();
                                linearGradientBrush2.EndPoint = new Point(0.0, 1.0);
                                brush = (Brush)linearGradientBrush2;
                            }
                            switch (aeroFreezables2 - 6) {
                                case DataGridHeaderBorder.AeroFreezables.NormalBevel:
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)242, (byte)242, (byte)242), 0.0));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)239, (byte)239, (byte)239), 0.4));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)231, (byte)232, (byte)234), 0.4));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)222, (byte)223, (byte)225), 1.0));
                                    break;
                                case DataGridHeaderBorder.AeroFreezables.NormalBackground:
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)122, (byte)158, (byte)177), 0.0));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)122, (byte)158, (byte)177), 0.4));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)80, (byte)145, (byte)175), 0.4));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)77, (byte)141, (byte)173), 1.0));
                                    break;
                                case DataGridHeaderBorder.AeroFreezables.PressedBackground:
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)136, (byte)203, (byte)235), 0.0));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)136, (byte)203, (byte)235), 0.4));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)105, (byte)187, (byte)227), 0.4));
                                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)105, (byte)187, (byte)227), 1.0));
                                    break;
                                case DataGridHeaderBorder.AeroFreezables.HoveredBackground:
                                    brush = (Brush)new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)150, (byte)217, (byte)249));
                                    break;
                            }
                            brush.Freeze();
                            DataGridHeaderBorder.CacheFreezable((Freezable)brush, (int)aeroFreezables2);
                        }
                    }
                    dc.DrawRectangle(brush, (Pen)null, new Rect(0.0, 0.0, 1.0, DataGridHeaderBorder.Max0(renderSize.Height - 0.95)));
                    dc.DrawRectangle(brush, (Pen)null, new Rect(renderSize.Width - 1.0, 0.0, 1.0, DataGridHeaderBorder.Max0(renderSize.Height - 0.95)));
                }
            }
            if (flag4 && renderSize.Width >= 4.0 && renderSize.Height >= 4.0) {
                LinearGradientBrush linearGradientBrush2 = (LinearGradientBrush)DataGridHeaderBorder.GetCachedFreezable(5);
                if (linearGradientBrush2 == null) {
                    linearGradientBrush2 = new LinearGradientBrush();
                    linearGradientBrush2.StartPoint = new Point();
                    linearGradientBrush2.EndPoint = new Point(0.0, 1.0);
                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)134, (byte)163, (byte)178), 0.0));
                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)134, (byte)163, (byte)178), 0.1));
                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)170, (byte)206, (byte)225), 0.9));
                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)170, (byte)206, (byte)225), 1.0));
                    linearGradientBrush2.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)linearGradientBrush2, 5);
                }
                dc.DrawRectangle((Brush)linearGradientBrush2, (Pen)null, new Rect(0.0, 0.0, renderSize.Width, 2.0));
                LinearGradientBrush linearGradientBrush3 = (LinearGradientBrush)DataGridHeaderBorder.GetCachedFreezable(10);
                if (linearGradientBrush3 == null) {
                    linearGradientBrush3 = new LinearGradientBrush();
                    linearGradientBrush3.StartPoint = new Point();
                    linearGradientBrush3.EndPoint = new Point(0.0, 1.0);
                    linearGradientBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)162, (byte)203, (byte)224), 0.0));
                    linearGradientBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)162, (byte)203, (byte)224), 0.4));
                    linearGradientBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)114, (byte)188, (byte)223), 0.4));
                    linearGradientBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)110, (byte)184, (byte)220), 1.0));
                    linearGradientBrush3.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)linearGradientBrush3, 10);
                }
                dc.DrawRectangle((Brush)linearGradientBrush3, (Pen)null, new Rect(1.0, 0.0, 1.0, renderSize.Height - 0.95));
                dc.DrawRectangle((Brush)linearGradientBrush3, (Pen)null, new Rect(renderSize.Width - 2.0, 0.0, 1.0, renderSize.Height - 0.95));
            }
            if (renderSize.Height >= 2.0) {
                DataGridHeaderBorder.AeroFreezables aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.NormalBottom;
                if (flag4)
                    aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.PressedOrHoveredBottom;
                else if (flag3)
                    aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.PressedOrHoveredBottom;
                else if (hasValue | isSelected)
                    aeroFreezables2 = DataGridHeaderBorder.AeroFreezables.SortedBottom;
                SolidColorBrush solidColorBrush = (SolidColorBrush)DataGridHeaderBorder.GetCachedFreezable((int)aeroFreezables2);
                if (solidColorBrush == null) {
                    switch (aeroFreezables2) {
                        case DataGridHeaderBorder.AeroFreezables.NormalBottom:
                            solidColorBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)213, (byte)213, (byte)213));
                            break;
                        case DataGridHeaderBorder.AeroFreezables.PressedOrHoveredBottom:
                            solidColorBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)147, (byte)201, (byte)227));
                            break;
                        case DataGridHeaderBorder.AeroFreezables.SortedBottom:
                            solidColorBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)150, (byte)217, (byte)249));
                            break;
                    }
                    solidColorBrush.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)solidColorBrush, (int)aeroFreezables2);
                }
                dc.DrawRectangle((Brush)solidColorBrush, (Pen)null, new Rect(0.0, renderSize.Height - 1.0, renderSize.Width, 1.0));
            }
            if (hasValue && renderSize.Width > 14.0 && renderSize.Height > 10.0) {
                TranslateTransform translateTransform = new TranslateTransform((renderSize.Width - 8.0) * 0.5, 1.0);
                translateTransform.Freeze();
                dc.PushTransform((Transform)translateTransform);
                ListSortDirection? nullable = sortDirection;
                ListSortDirection listSortDirection = ListSortDirection.Ascending;
                bool flag6 = nullable.GetValueOrDefault() == listSortDirection && nullable.HasValue;
                PathGeometry pathGeometry = (PathGeometry)DataGridHeaderBorder.GetCachedFreezable(flag6 ? 17 : 18);
                if (pathGeometry == null) {
                    pathGeometry = new PathGeometry();
                    PathFigure pathFigure = new PathFigure();
                    if (flag6) {
                        pathFigure.StartPoint = new Point(0.0, 4.0);
                        LineSegment lineSegment1 = new LineSegment(new Point(4.0, 0.0), false);
                        lineSegment1.Freeze();
                        pathFigure.Segments.Add((PathSegment)lineSegment1);
                        LineSegment lineSegment2 = new LineSegment(new Point(8.0, 4.0), false);
                        lineSegment2.Freeze();
                        pathFigure.Segments.Add((PathSegment)lineSegment2);
                    } else {
                        pathFigure.StartPoint = new Point(0.0, 0.0);
                        LineSegment lineSegment1 = new LineSegment(new Point(8.0, 0.0), false);
                        lineSegment1.Freeze();
                        pathFigure.Segments.Add((PathSegment)lineSegment1);
                        LineSegment lineSegment2 = new LineSegment(new Point(4.0, 4.0), false);
                        lineSegment2.Freeze();
                        pathFigure.Segments.Add((PathSegment)lineSegment2);
                    }
                    pathFigure.IsClosed = true;
                    pathFigure.Freeze();
                    pathGeometry.Figures.Add(pathFigure);
                    pathGeometry.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)pathGeometry, flag6 ? 17 : 18);
                }
                LinearGradientBrush linearGradientBrush2 = (LinearGradientBrush)DataGridHeaderBorder.GetCachedFreezable(14);
                if (linearGradientBrush2 == null) {
                    linearGradientBrush2 = new LinearGradientBrush();
                    linearGradientBrush2.StartPoint = new Point();
                    linearGradientBrush2.EndPoint = new Point(1.0, 1.0);
                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)60, (byte)94, (byte)114), 0.0));
                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)60, (byte)94, (byte)114), 0.1));
                    linearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)195, (byte)228, (byte)245), 1.0));
                    linearGradientBrush2.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)linearGradientBrush2, 14);
                }
                dc.DrawGeometry((Brush)linearGradientBrush2, (Pen)null, (Geometry)pathGeometry);
                LinearGradientBrush linearGradientBrush3 = (LinearGradientBrush)DataGridHeaderBorder.GetCachedFreezable(15);
                if (linearGradientBrush3 == null) {
                    linearGradientBrush3 = new LinearGradientBrush();
                    linearGradientBrush3.StartPoint = new Point();
                    linearGradientBrush3.EndPoint = new Point(1.0, 1.0);
                    linearGradientBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)97, (byte)150, (byte)182), 0.0));
                    linearGradientBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)97, (byte)150, (byte)182), 0.1));
                    linearGradientBrush3.GradientStops.Add(new GradientStop(Color.FromArgb(byte.MaxValue, (byte)202, (byte)230, (byte)245), 1.0));
                    linearGradientBrush3.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)linearGradientBrush3, 15);
                }
                ScaleTransform scaleTransform = (ScaleTransform)DataGridHeaderBorder.GetCachedFreezable(16);
                if (scaleTransform == null) {
                    scaleTransform = new ScaleTransform(0.75, 0.75, 3.5, 4.0);
                    scaleTransform.Freeze();
                    DataGridHeaderBorder.CacheFreezable((Freezable)scaleTransform, 16);
                }
                dc.PushTransform((Transform)scaleTransform);
                dc.DrawGeometry((Brush)linearGradientBrush3, (Pen)null, (Geometry)pathGeometry);
                dc.Pop();
                dc.Pop();
            }
            if (!flag1)
                return;
            dc.Pop();
        }

        private enum AeroFreezables
        {
            NormalBevel,
            NormalBackground,
            PressedBackground,
            HoveredBackground,
            SortedBackground,
            PressedTop,
            NormalSides,
            PressedSides,
            HoveredSides,
            SortedSides,
            PressedBevel,
            NormalBottom,
            PressedOrHoveredBottom,
            SortedBottom,
            ArrowBorder,
            ArrowFill,
            ArrowFillScale,
            ArrowUpGeometry,
            ArrowDownGeometry,
            NumFreezables,
        }
    }
}
