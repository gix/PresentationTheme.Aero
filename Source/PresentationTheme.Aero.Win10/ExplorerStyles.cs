namespace PresentationTheme.Aero.Win10
{
    using System.Windows;
    using System.Windows.Controls;

    public static class ExplorerStyles
    {
        private static ResourceKey listViewStyleKey;
        private static ResourceKey listViewItemStyleKey;
        private static ResourceKey treeViewStyleKey;
        private static ResourceKey treeViewItemStyleKey;

        public static ResourceKey ListViewStyleKey =>
            listViewStyleKey ??
            (listViewStyleKey = new ComponentResourceKey(typeof(ListView), "Explorer"));

        public static ResourceKey ListViewItemStyleKey =>
            listViewItemStyleKey ??
            (listViewItemStyleKey = new ComponentResourceKey(typeof(ListViewItem), "Explorer"));

        public static ResourceKey TreeViewStyleKey =>
            treeViewStyleKey ??
            (treeViewStyleKey = new ComponentResourceKey(typeof(TreeView), "Explorer"));

        public static ResourceKey TreeViewItemStyleKey =>
            treeViewItemStyleKey ??
            (treeViewItemStyleKey = new ComponentResourceKey(typeof(TreeViewItem), "Explorer"));
    }
}
