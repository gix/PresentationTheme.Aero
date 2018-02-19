namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media.Animation;
    using Aero;

    /// <summary>
    ///   Provides improved animation styling for <see cref="ProgressBar"/>
    ///   controls.
    /// </summary>
    [TemplatePart(Name = "PART_Track", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_Indicator", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_MoveOverlay", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_PulseOverlay", Type = typeof(FrameworkElement))]
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

        static ProgressBarChrome()
        {
            FocusableProperty.OverrideMetadata(
                typeof(ProgressBarChrome), new FrameworkPropertyMetadata(false));
            MaximumProperty.OverrideMetadata(
                typeof(ProgressBarChrome), new FrameworkPropertyMetadata(100.0));
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ProgressBarChrome"/>
        ///   class.
        /// </summary>
        public ProgressBarChrome()
        {
            IsVisibleChanged += (s, e) => UpdateHighlightAnimation();
            SystemVisualStateManager.Instance.AnimatesChanged +=
                (s, e) => UpdateHighlightAnimation();
        }

        /// <summary>
        ///   Identifies the <see cref="IsIndeterminate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsIndeterminate),
                typeof(bool),
                typeof(ProgressBarChrome),
                new FrameworkPropertyMetadata(
                    false, OnIsIndeterminateChanged));

        /// <summary>
        ///   Gets or sets whether the <see cref="ProgressBarChrome"/> shows
        ///   actual values or generic, continuous progress feedback.
        /// </summary>
        /// <value>
        ///   <see langword="false"/> if the <see cref="ProgressBarChrome"/>
        ///   shows actual values; <see langword="true"/> if the
        ///   <see cref="ProgressBarChrome"/> shows generic progress. The default
        ///   is <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="IsIndeterminateProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>None</description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
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

        /// <summary>
        ///   Gets or sets the orientation of the progress bar chrome.
        /// </summary>
        /// <value>
        ///   An <see cref="System.Windows.Controls.Orientation"/> enumeration
        ///   value that defines whether the <see cref="ProgressBarChrome"/> is
        ///   displayed horizontally or vertically. The default is
        ///   <see cref="System.Windows.Controls.Orientation.Horizontal"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="OrientationProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsMeasure"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            SetIndicatorLength(animate: true);
        }

        /// <inheritdoc/>
        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            SetIndicatorLength();
        }

        /// <inheritdoc/>
        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            SetIndicatorLength();
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
            if (!isReverse && animate && SystemVisualStateManager.Instance.Animates) {
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
            if (IsIndeterminate || SystemVisualStateManager.Instance.Animates) {
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
