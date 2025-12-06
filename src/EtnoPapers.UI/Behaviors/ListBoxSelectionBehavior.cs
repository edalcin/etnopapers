using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace EtnoPapers.UI.Behaviors
{
    /// <summary>
    /// Attached behavior to synchronize ListBox selection with a ViewModel collection.
    /// Usage: local:ListBoxSelectionBehavior.SelectedItems="{Binding SelectedRecords}"
    ///
    /// IMPORTANT: This behavior uses a guard flag to prevent infinite synchronization loops.
    /// </summary>
    public static class ListBoxSelectionBehavior
    {
        private static bool _isSyncing = false;

        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListBoxSelectionBehavior),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox && !_isSyncing)
            {
                try
                {
                    _isSyncing = true;

                    // Unsubscribe first to prevent duplicate handlers
                    listBox.SelectionChanged -= ListBox_SelectionChanged;
                    listBox.SelectionChanged += ListBox_SelectionChanged;

                    // Sync ViewModel collection to UI only on initial binding
                    if (e.NewValue is IList selectedItems && e.OldValue == null)
                    {
                        listBox.SelectedItems.Clear();
                        foreach (var item in selectedItems)
                        {
                            listBox.SelectedItems.Add(item);
                        }
                    }
                }
                finally
                {
                    _isSyncing = false;
                }
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSyncing || !(sender is ListBox listBox))
                return;

            try
            {
                _isSyncing = true;
                var selectedItems = GetSelectedItems(listBox);

                if (selectedItems != null)
                {
                    // Sync UI selection to ViewModel collection
                    selectedItems.Clear();
                    foreach (var item in listBox.SelectedItems)
                    {
                        selectedItems.Add(item);
                    }
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
