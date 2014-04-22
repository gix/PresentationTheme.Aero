namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    ///   Provides an extended <see cref="VisualStateManager"/> that honors system-wide
    ///   animation settings and hardware capabilities. Animations are used if:
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <see cref="SystemParameters.ClientAreaAnimation"/> is
    ///         <see langword="true"/>,
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="RenderCapability.Tier"/> is <c>1</c> or higher, and
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="RenderCapability.Tier"/> is <c>1</c> or higher
    ///       </description>
    ///     </item>
    ///   </list>
    /// </summary>
    public sealed class DynamicVisualStateManager : VisualStateManager
    {
        private static readonly Lazy<DynamicVisualStateManager> LazyInstance =
            new Lazy<DynamicVisualStateManager>(() => new DynamicVisualStateManager());

        public static DynamicVisualStateManager Instance => LazyInstance.Value;

        private DynamicVisualStateManager()
        {
            SystemParameters.StaticPropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation))
                    RaiseAnimatesChanged();
            };

            RenderCapability.TierChanged += (s, e) => RaiseAnimatesChanged();
        }

        private bool? useAnimationsOverride;

        /// <summary>
        ///   Gets or sets a value determining whether animations are forcibly
        ///   enabled or disabled.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to forcibly enable animations.
        ///   <see langword="false"/> to disable animations.
        ///   Use <see langword="null"/> to automatically determine whether
        ///   animations should be used.
        /// </value>
        public bool? UseAnimationsOverride
        {
            get { return useAnimationsOverride; }
            set
            {
                if (useAnimationsOverride != value) {
                    useAnimationsOverride = value;
                    RaiseAnimatesChanged();
                }
            }
        }

        protected override bool GoToStateCore(
            FrameworkElement control, FrameworkElement stateGroupsRoot,
            string stateName, VisualStateGroup group, VisualState state,
            bool useTransitions)
        {
            return
                state != null &&
                base.GoToStateCore(
                    control, stateGroupsRoot, stateName, group, state,
                    useTransitions && Animates);
        }

        public bool Animates =>
            AeroWin10Theme.UseAnimationsOverride.GetValueOrDefault(
                SystemParameters.ClientAreaAnimation &&
                RenderCapability.Tier > 0);

        public event EventHandler AnimatesChanged;

        private void RaiseAnimatesChanged()
        {
            AnimatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
