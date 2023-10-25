using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WoWonderDesktop.Helpers.Utils;

namespace WoWonderDesktop.Library
{
    public class AnjoListBoxScrollListener
    {
        private ScrollViewer ScrollViewer;
        private IListBoxOnScrollListener Listener;
        private readonly ListBox XListBox;
        public bool IsLoading { get; set; }

        public interface IListBoxOnScrollListener
        {
            public void OnLoadMore();
            public void OnLoadUp();
        }

        public AnjoListBoxScrollListener(ListBox listBox)
        {
            try
            {
                IsLoading = false;
                XListBox = listBox;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetScrollListener(IListBoxOnScrollListener listener)
        {
            try
            {
                Listener = listener;

                //get the ScrollViewer from the ListBox
                ScrollViewer = GetDescendantByType(XListBox, typeof(ScrollViewer)) as ScrollViewer;

                //attach to custom binding to check if ScrollViewer verticalOffset property has changed
                if (ScrollViewer != null)
                {
                    var binding = new Binding("VerticalOffset") { Source = ScrollViewer };
                    var offsetChangeListener = DependencyProperty.RegisterAttached("ListenerOffset_" + XListBox.Name + "" + Methods.FunString.RandomString(3), typeof(object), typeof(UserControl), new PropertyMetadata(OnScrollChanged));
                    ScrollViewer.SetBinding(offsetChangeListener, binding);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        private Visual GetDescendantByType(Visual element, Type type)
        {
            try
            {
                if (element == null)
                    return null;

                if (element.GetType() == type)
                    return element;

                Visual foundElement = null;
                if (element is FrameworkElement frameworkElement)
                {
                    frameworkElement.ApplyTemplate();
                }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                {
                    Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                    foundElement = GetDescendantByType(visual, type);
                    if (foundElement != null)
                    {
                        break;
                    }
                }

                return foundElement;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        private void OnScrollChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                //We have to check if the values are 0.0 because they are both set to this when the ScrollViewer loads
                if (ScrollViewer.ScrollableHeight <= ScrollViewer.VerticalOffset && ScrollViewer.ScrollableHeight != 0.0 && ScrollViewer.VerticalOffset != 0.0)
                {
                    if (IsLoading)
                        return;

                    //The ScrollBar is at the bottom, load more results.
                    Listener?.OnLoadMore();
                }
                else if (ScrollViewer.ScrollableHeight >= ScrollViewer.VerticalOffset && ScrollViewer.ScrollableHeight != 0.0 && ScrollViewer.VerticalOffset == 0.0)
                {
                    if (IsLoading)
                        return;

                    //The ScrollBar is at the top, load more results.
                    Listener?.OnLoadUp();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}
