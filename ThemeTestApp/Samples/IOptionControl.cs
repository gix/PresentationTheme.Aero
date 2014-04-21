namespace ThemeTestApp.Samples
{
    using System.Collections.Generic;

    public interface IOptionControl
    {
        IReadOnlyList<Option> Options { get; }
    }
}
