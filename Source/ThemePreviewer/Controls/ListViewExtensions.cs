namespace ThemePreviewer.Controls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ListViewExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public Mask mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4,       // HDI_FORMAT
            }

            [Flags]
            public enum Format
            {
                SortDown = 0x200,   // HDF_SORTDOWN
                SortUp = 0x400,     // HDF_SORTUP
            }
        }

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = LVM_FIRST + 31;

        public const int HDM_FIRST = 0x1200;
        public const int HDM_GETITEM = HDM_FIRST + 11;
        public const int HDM_SETITEM = HDM_FIRST + 12;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(
            IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(
            IntPtr hWnd, uint msg, IntPtr wParam, ref HDITEM lParam);


        private class ItemComparer : IComparer
        {
            private readonly int columnIndex;

            public ItemComparer(int columnIndex)
            {
                this.columnIndex = columnIndex;
            }

            public int Compare(object x, object y)
            {
                return Compare(x as ListViewItem, y as ListViewItem);
            }

            public int Compare(ListViewItem x, ListViewItem y)
            {
                var leftNull = x == null || x.SubItems.Count <= columnIndex;
                var rightNull = y == null || y.SubItems.Count <= columnIndex;
                if (leftNull)
                    return rightNull ? 0 : 1;
                if (rightNull)
                    return -1;

                return string.CompareOrdinal(
                    x.SubItems[columnIndex].Text, y.SubItems[columnIndex].Text);
            }
        }

        public static void ToggleSort(
            this ListView listView, int columnIndex)
        {
            var currSortOrder = listView.GetSortIcon(columnIndex);
            SortOrder newSortOrder;
            if (currSortOrder == SortOrder.Ascending)
                newSortOrder = SortOrder.Descending;
            else
                newSortOrder = SortOrder.Ascending;

            Sort(listView, columnIndex, newSortOrder);
        }

        public static void Sort(
            this ListView listView, int columnIndex, SortOrder order)
        {
            listView.ListViewItemSorter = new ItemComparer(columnIndex);
            listView.SetSortIcon(columnIndex, order);
        }

        public static SortOrder GetSortIcon(this ListView listView, int columnIndex)
        {
            IntPtr columnHeader = SendMessage(
                listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            var item = new HDITEM {
                mask = HDITEM.Mask.Format
            };

            if (SendMessage(columnHeader, HDM_GETITEM, new IntPtr(columnIndex), ref item) == IntPtr.Zero)
                throw new Win32Exception();

            if ((item.fmt & HDITEM.Format.SortUp) != 0)
                return SortOrder.Ascending;
            if ((item.fmt & HDITEM.Format.SortDown) != 0)
                return SortOrder.Descending;

            return SortOrder.None;
        }

        public static void SetSortIcon(
            this ListView listView, int columnIndex, SortOrder order)
        {
            IntPtr columnHeader = SendMessage(
                listView.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            for (int column = 0; column <= listView.Columns.Count - 1; ++column) {
                var columnPtr = new IntPtr(column);
                var item = new HDITEM {
                    mask = HDITEM.Mask.Format
                };

                if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
                    throw new Win32Exception();

                if (order != SortOrder.None && column == columnIndex) {
                    switch (order) {
                        case SortOrder.Ascending:
                            item.fmt &= ~HDITEM.Format.SortDown;
                            item.fmt |= HDITEM.Format.SortUp;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~HDITEM.Format.SortUp;
                            item.fmt |= HDITEM.Format.SortDown;
                            break;
                    }
                } else {
                    item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
                }

                if (SendMessage(columnHeader, HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero)
                    throw new Win32Exception();
            }
        }
    }
}
