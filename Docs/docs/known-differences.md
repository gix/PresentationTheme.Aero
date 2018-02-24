Known Differences of the Windows 10 Aero Theme
==============================================

While the WPF theme strives to be as close as possible to the native theme there
are minor differences. Usually these are the result of avoiding a more expensive
implementation (e.g. reproducing all rounding issues) or improve upon certain
inconsistencies and flaws of the native theme.


TextBox
-------

Native textboxes have their text vertically 1px above center.

[!cmp[Native](../images/diff-edit-native.png)[WPF](../images/diff-edit-wpf.png)]


ComboBox
--------

Native comboboxes have their text vertically 1px above center.

[!cmp[Native](../images/diff-combo-native.png)[WPF](../images/diff-combo-wpf.png)]

Disabled comboboxes have a smaller inner white border.

[!cmp[Native](../images/diff-combo-disabled-native.png)[WPF](../images/diff-combo-disabled-wpf.png)]


Menus
-----

Native disabled menu items have a mouseover state. In WPF such menu items have
their [IsEnabled](xref:System.Windows.UIElement.IsEnabled) property set to false
and do not receive input events.


TabControl
----------

The native TabControl has the last tab cut short and a 2px shadow-like right
and bottom border.

[!cmp[Native](../images/diff-tab-native.png)[WPF](../images/diff-tab-wpf.png)]


ListView
--------

Explorer-style ListView has no vertical gridlines compared to the native theme.
Also header items lack ellipsis for long text and have no sort indicator because
the control does not support it.

[!cmp[Native](../images/diff-listview-native.png)[WPF](../images/diff-listview-wpf.png)]

Explorer-style ListView uses the same system color for fonts as the normal
ListView. The native ListView uses a different one. This is usually irrelevant
since themes do not have different values for these colors.


TreeView
--------

Both TreeViews have the expander icon vertically aligned but for the default
height the native theme is 1px off.

[!cmp[Native](../images/diff-treeview-native.png)[WPF](../images/diff-treeview-wpf.png)]

Explorer-style TreeView uses the same system color for fonts as the normal
TreeView. The native TreeView uses a different one. This is usually irrelevant
since themes do not have different values for these colors.


Slider/Trackbar
---------------

The selection bars have slightly different lengths due to rounding.

[!cmp[Native](../images/diff-trackbar-native.png)[WPF](../images/diff-trackbar-wpf.png)]


StatusBar
---------

StatusBar items with separators have a slighty different width if the same
widths are specified because the native theme treats separators as part of an
item while WPF uses [Separators](xref:System.Windows.Controls.Separator).


Known Differences of the Windows 8/8.1 Aero Theme
=================================================

The differences are similar to those described for the Windows 10 theme.
