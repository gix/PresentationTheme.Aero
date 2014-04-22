namespace ThemePreviewer
{
    using System.Collections.Generic;

    public interface IOptionControl
    {
        IReadOnlyList<Option> Options { get; }
    }
}
