using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace EtnoPapers.UI.Behaviors
{
    /// <summary>
    /// Attached behavior to synchronize ListBox selection with a ViewModel collection.
    /// Usage: local:ListBoxSelectionBehavior.SelectedItems="{Binding SelectedRecords}"
    /// </summary>
    public static class ListBoxSelectionBehavior
    {
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
            if (d is ListBox listBox)
            {
                listBox.SelectionChanged -= ListBox_SelectionChanged;
                listBox.SelectionChanged += ListBox_SelectionChanged;

                // Sync ViewModel collection to UI
                if (e.NewValue is IList selectedItems)
                {
                    listBox.SelectedItems.Clear();
                    foreach (var item in selectedItems)
                    {
                        listBox.SelectedItems.Add(item);
                    }
                }
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
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
        }
    }
}
