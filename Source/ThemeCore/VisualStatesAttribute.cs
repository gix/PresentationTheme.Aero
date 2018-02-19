namespace ThemeCore
{
    using System;

    internal class VisualStatesAttribute : Attribute
    {
        public VisualStatesAttribute(Type statesEnumType)
        {
            StatesEnumType = statesEnumType;
        }

        public Type StatesEnumType { get; }
    }
}
