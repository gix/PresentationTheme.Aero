namespace PresentationTheme.Aero
{
    using System;

    /// <summary>
    ///   Defines which theme resources are used for a WPF control assembly.
    /// </summary>
    /// <seealso cref="ThemeManager.SetTheme(System.Reflection.Assembly,IThemePolicy)"/>
    public interface IThemePolicy
    {
        /// <summary>
        ///   Gets or sets a value indicating whether the resources provided by
        ///   <see cref="GetCurrentThemeUri"/> are merged with the base resources
        ///   of the assembly this policy is used for.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> to merge the resources with the base resources.
        ///   The actual theme resource dictionary provided to WPF will have the
        ///   base resources and the resources provided by this policy as merged
        ///   resource dictionaries. <see langword="false"/> to replace the base
        ///   resources and only use the resources provided by this policy. In
        ///   this case ensure that all expected resource keys are present.
        /// </value>
        bool MergeWithBaseResources { get; }

        /// <summary>
        ///   Gets the pack <see cref="Uri"/> to the location of the theme resource
        ///   dictionary to use. This method will be invoked again to get an
        ///   updated resource URI in case the system theme has changed. If the
        ///   method returns <see langword="null"/> the default theme resources
        ///   are used.
        /// </summary>
        Uri GetCurrentThemeUri();
    }
}
