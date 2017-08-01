namespace PresentationTheme.Aero
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    ///   Provides an extended <see cref="VisualStateManager"/> that honors
    ///   system-wide animation settings and hardware capabilities. Animations
    ///   are used by default if:
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <see cref="SystemParameters.ClientAreaAnimation"/> is
    ///         <see langword="true"/>, and
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <see cref="RenderCapability.Tier"/> is <c>1</c> or higher.
    ///       </description>
    ///     </item>
    ///   </list>
    ///   Animations can be forcibly enabled or disabled regardless of system
    ///   settings by setting <see cref="UseAnimationsOverride"/>. Use
    ///   <see cref="Instance"/> to retrieve a shared instance of the state
    ///   manager.
    /// </summary>
    public sealed class SystemVisualStateManager : VisualStateManager
    {
        private static readonly Lazy<SystemVisualStateManager> LazyInstance =
            new Lazy<SystemVisualStateManager>(() => new SystemVisualStateManager());

        /// <summary>
        ///   Gets the global instance of the <see cref="SystemVisualStateManager"/>.
        /// </summary>
        public static SystemVisualStateManager Instance => LazyInstance.Value;

        private bool? useAnimationsOverride;

        private SystemVisualStateManager()
        {
            SystemParameters.StaticPropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation))
                    RaiseAnimatesChanged();
            };

            RenderCapability.TierChanged += (s, e) => RaiseAnimatesChanged();
        }

        /// <summary>
        ///   Gets a value indicating whether animations are used for state
        ///   transitions.
        /// </summary>
        /// <seealso cref="UseAnimationsOverride"/>
        public bool Animates =>
            UseAnimationsOverride.GetValueOrDefault(
                SystemParameters.ClientAreaAnimation &&
                RenderCapability.Tier > 0);

        /// <summary>
        ///   Occurs when the value of the <see cref="Animates"/> property has
        ///   changed.
        /// </summary>
        public event EventHandler AnimatesChanged;

        /// <summary>
        ///   Gets or sets a value determining whether animations are forcibly
        ///   enabled or disabled.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to forcibly enable animations.
        ///   <see langword="false"/> to disable animations. Use <see langword="null"/>
        ///   to automatically determine whether animations should be used.
        /// </value>
        public bool? UseAnimationsOverride
        {
            get => useAnimationsOverride;
            set
            {
                if (useAnimationsOverride != value) {
                    bool oldAnimates = Animates;
                    useAnimationsOverride = value;
                    if (Animates != oldAnimates)
                        RaiseAnimatesChanged();
                }
            }
        }

        /// <inheritdoc/>
        protected override bool GoToStateCore(
            FrameworkElement control, FrameworkElement stateGroupsRoot,
            string stateName, VisualStateGroup group, VisualState state,
            bool useTransitions)
        {
            return
                // A custom VisualStateManager is called even if state is null.
                state != null &&
                base.GoToStateCore(
                    control, stateGroupsRoot, stateName, group, state,
                    useTransitions && Animates);
        }

        private void RaiseAnimatesChanged()
        {
            AnimatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
