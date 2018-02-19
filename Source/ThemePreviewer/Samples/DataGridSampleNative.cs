namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Forms;

    public partial class DataGridSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options;

        public DataGridSampleNative()
        {
            InitializeComponent();
            grid.DataSource = new SortableBindingList<Item>(new ItemsCollection().ToList());

            options = new OptionList();
            options.AddOption("Enabled", grid, t => t.Enabled);
            options.AddOption("Multiple Rows", grid, c => c.MultiSelect);
            options.AddOption("Full Row Select", grid, c => c.SelectionMode,
                DataGridViewSelectionMode.FullRowSelect,
                DataGridViewSelectionMode.RowHeaderSelect);
            options.AddOption("Show Grid Lines", grid, c => c.CellBorderStyle,
                DataGridViewCellBorderStyle.Single,
                DataGridViewCellBorderStyle.None);
        }

        public IReadOnlyList<Option> Options => options;

        public class SortableBindingList<T> : BindingList<T> where T : class
        {
            private bool isSorted;
            private ListSortDirection sortDirection = ListSortDirection.Ascending;
            private PropertyDescriptor sortProperty;
            private IComparer comparer;

            public SortableBindingList()
            {
            }

            public SortableBindingList(IList<T> list)
                : base(list)
            {
            }

            protected override bool SupportsSortingCore => true;
            protected override bool IsSortedCore => isSorted;
            protected override ListSortDirection SortDirectionCore => sortDirection;
            protected override PropertyDescriptor SortPropertyCore => sortProperty;

            protected override void ApplySortCore(
                PropertyDescriptor property, ListSortDirection direction)
            {
                sortProperty = property;
                sortDirection = direction;
                comparer = CreateComparer(property.PropertyType);

                if (!(Items is List<T> list))
                    return;

                list.Sort(Compare);

                isSorted = true;
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }

            private static IComparer CreateComparer(Type valueType)
            {
                var comparerType = typeof(Comparer<>).MakeGenericType(valueType);
                var defaultProperty = comparerType.GetProperty(nameof(Comparer<object>.Default));
                return (IComparer)defaultProperty.GetValue(null);
            }

            protected override void RemoveSortCore()
            {
                isSorted = false;
                sortDirection = ListSortDirection.Ascending;
                sortProperty = null;
                comparer = null;
            }

            private int Compare(T lhs, T rhs)
            {
                var cmp = comparer.Compare(
                    sortProperty.GetValue(lhs), sortProperty.GetValue(rhs));
                if (sortDirection == ListSortDirection.Descending)
                    cmp = -cmp;
                return cmp;
            }
        }
    }
}
