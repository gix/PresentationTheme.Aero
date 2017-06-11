namespace ThemeCore
{
    using System;

    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
    internal class VisualClassAttribute : Attribute
    {
        public VisualClassAttribute(string className)
        {
            ClassName = className;
        }

        public string ClassName { get; }
    }
}
