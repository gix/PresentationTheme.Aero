namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media.Animation;

    /// <summary>
    ///   Provides improved animation styling for <see cref="ProgressBar"/>
    ///   controls.
    /// </summary>
    public class ProgressBarChrome : RangeBase
    {
        // Property values from aero.msstyles
        private const int AnimationDelay = 1000; // ms
        private const int AnimationDuration = 500; // ms

        private FrameworkElement track;
        private FrameworkElement indicator;
        private FrameworkElement moveOverlay;
        private FrameworkElement pulseOverlay;

        private Storyboard highlightStoryboard;

        public ProgressBarChrome()
        {
            IsVisibleChanged += (s, e) => UpdateHighlightAnimation();
            DynamicVisualStateManager.Instance.AnimatesChanged +=
                (s, e) => UpdateHighlightAnimation();
        }

        public static readonly DependencyProperty BarProperty =
            DependencyProperty.Register(
                nameof(Bar),
                typeof(ProgressBar),
                typeof(ProgressBarChrome),
                new PropertyMetadata(null, OnProgressBarChanged));

        public ProgressBar Bar
        {
            get => (ProgressBar)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }

        static ProgressBarChrome()
        {
            FocusableProperty.OverrideMetadata(
                typeof(ProgressBarChrome), new FrameworkPropertyMetadata(false));
            MaximumProperty.OverrideMetadata(
                typeof(ProgressBarChrome), new FrameworkPropertyMetadata(100.0));
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsIndeterminate),
                typeof(bool),
                typeof(ProgressBarChrome),
                new FrameworkPropertyMetadata(
                    false, OnIsIndeterminateChanged));

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(ProgressBarChrome),
                new FrameworkPropertyMetadata(
                    Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOrientationChanged),
                IsValidOrientation);

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (track != null)
                track.SizeChanged -= OnTrackSizeChanged;

            track = GetTemplateChild("PART_Track") as FrameworkElement;
            indicator = GetTemplateChild("PART_Indicator") as FrameworkElement;
            moveOverlay = GetTemplateChild("PART_MoveOverlay") as FrameworkElement;
            pulseOverlay = GetTemplateChild("PART_PulseOverlay") as FrameworkElement;

            if (track != null)
                track.SizeChanged += OnTrackSizeChanged;
            if (IsIndeterminate)
                UpdateHighlightAnimation(true);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            SetIndicatorLength(animate: true);
        }

        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            SetIndicatorLength();
        }

        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            SetIndicatorLength();
        }

        private static void OnProgressBarChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ProgressBarChrome)d;

            var newValue = (ProgressBar)e.NewValue;
            if (newValue != null) {
                source.SetBinding(ValueProperty, new Binding(nameof(Value)) { Source = newValue });
                source.SetBinding(MinimumProperty, new Binding(nameof(Minimum)) { Source = newValue });
                source.SetBinding(MaximumProperty, new Binding(nameof(Maximum)) { Source = newValue });
                source.SetBinding(OrientationProperty, new Binding(nameof(Orientation)) { Source = newValue });
                source.SetBinding(IsIndeterminateProperty, new Binding(nameof(IsIndeterminate)) { Source = newValue });
                source.SetBinding(ForegroundProperty, new Binding(nameof(Foreground)) { Source = newValue });
            } else {
                BindingOperations.ClearBinding(source, ValueProperty);
                BindingOperations.ClearBinding(source, MinimumProperty);
                BindingOperations.ClearBinding(source, MaximumProperty);
                BindingOperations.ClearBinding(source, OrientationProperty);
                BindingOperations.ClearBinding(source, IsIndeterminateProperty);
                BindingOperations.ClearBinding(source, ForegroundProperty);
            }
        }

        private static void OnIsIndeterminateChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ProgressBarChrome)d;
            source.SetIndicatorLength(resetHightlight: true);
        }

        private static void OnOrientationChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ProgressBarChrome)d;
            source.SetIndicatorLength();
        }

        private static bool IsValidOrientation(object o)
        {
            var value = (Orientation)o;
            return value == Orientation.Horizontal
                   || value == Orientation.Vertical;
        }

        private void OnTrackSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetIndicatorLength();
        }

        private void SetIndicatorLength(bool animate = false, bool resetHightlight = false)
        {
            if (track == null || indicator == null)
                return;

            double minimum = Minimum;
            double maximum = Maximum;
            double value = Value;

            double scale;
            if (!IsIndeterminate && maximum > minimum)
                scale = (value - minimum) / (maximum - minimum);
            else
                scale = 1.0;

            var targetWidth = track.ActualWidth * scale;

            indicator.Width = targetWidth;

            bool isReverse = targetWidth < indicator.Width;
            if (!isReverse && animate && DynamicVisualStateManager.Instance.Animates) {
                var animation = new DoubleAnimation(
                    indicator.ActualWidth, targetWidth,
                    TimeSpan.FromMilliseconds(AnimationDuration));
                indicator.BeginAnimation(
                    WidthProperty, animation, HandoffBehavior.SnapshotAndReplace);
            } else {
                indicator.BeginAnimation(WidthProperty, null);
            }

            UpdateHighlightAnimation(resetHightlight);
        }

        private void UpdateHighlightAnimation(bool reset = false)
        {
            if (highlightStoryboard != null) {
                highlightStoryboard.Remove(this);
                highlightStoryboard = null;
            }

            if (moveOverlay == null)
                return;

            double indicatorWidth = (double)indicator.GetAnimationBaseValue(WidthProperty);

            if (!IsVisible || moveOverlay.Width <= 0 || indicatorWidth <= 0)
                return;

            // The native control renders 12px every frame with a timer
            // approximating 30 frames/sec but 360pps looks too fast.
            var pixelsPerSecond = IsIndeterminate ? 191.0 : 250.0;
            var delay = IsIndeterminate ? TimeSpan.Zero : TimeSpan.FromMilliseconds(AnimationDelay);

            // Set up the animation
            double startOffset = -moveOverlay.Width;
            double endOffset = indicatorWidth;
            double currentOffset = moveOverlay.Margin.Left;

            var translateTime = TimeSpan.FromSeconds((endOffset - startOffset) / pixelsPerSecond);
            var halfTranslateTime = TimeSpan.FromSeconds(translateTime.TotalSeconds / 2);
            var duration = new Duration(delay + translateTime);

            // Is the animation currenly running (with one pixel fudge factor)
            bool isAnimating = currentOffset > startOffset && currentOffset < endOffset - 1;

            TimeSpan startTime;
            if (isAnimating && !reset) {
                // make it appear that the timer already started.
                // To do this find out how many pixels the glow has moved and divide by the speed to get time.
                startTime = -delay - TimeSpan.FromSeconds((currentOffset - startOffset) / pixelsPerSecond);
            } else {
                startTime = TimeSpan.Zero;
            }

            var storyboard = new Storyboard();
            if (DynamicVisualStateManager.Instance.Animates) {
                var animation = new ThicknessAnimationUsingKeyFrames {
                    BeginTime = startTime,
                    Duration = duration,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                animation.KeyFrames.Add(
                    new LinearThicknessKeyFrame(
                        new Thickness(startOffset, 0, 0, 0),
                        TimeSpan.Zero));
                animation.KeyFrames.Add(
                    new LinearThicknessKeyFrame(
                        new Thickness(startOffset, 0, 0, 0),
                        delay));
                animation.KeyFrames.Add(
                    new LinearThicknessKeyFrame(
                        new Thickness(endOffset, 0, 0, 0),
                        delay + translateTime));

                Storyboard.SetTarget(animation, moveOverlay);
                Storyboard.SetTargetProperty(animation, new PropertyPath(MarginProperty));
                storyboard.Children.Add(animation);
            }

            // The pulse overlay is shown even if animations are disabled.
            if (pulseOverlay != null) {
                var animation = new DoubleAnimationUsingKeyFrames {
                    BeginTime = startTime,
                    Duration = duration,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                animation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(0, delay));
                animation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(1, delay + halfTranslateTime));
                animation.KeyFrames.Add(
                    new LinearDoubleKeyFrame(0, delay + translateTime));

                Storyboard.SetTarget(animation, pulseOverlay);
                Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
                storyboard.Children.Add(animation);
            }

            if (storyboard.Children.Count > 0) {
                storyboard.Begin(this, true);
                highlightStoryboard = storyboard;
            }
        }
    }
}
