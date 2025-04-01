using crs.dialog.ViewModels;
using crs.extension;
using crs.extension.Controls;
using HandyControl.Tools;
using System.Windows;
using System.Windows.Controls;
using crs.theme.Extensions;
using crs.extension.Models;
using System.Linq;
using System.Windows.Interop;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for SubEvaluateStandardPanel
    /// </summary>
    public partial class SubEvaluateStandardPanel : UserControl
    {
        public SubEvaluateStandardPanel()
        {
            InitializeComponent();
        }

        private async void completeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = subjectContentControl.DataContext as EvaluateStandardPanelViewModel;
            if (viewModel == null)
            {
                return;
            }

            var selectedItem = viewModel.SubjectSelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var _childrenItems = selectedItem.ChildrenItems;
            if (_childrenItems == null || _childrenItems.Count == 0)
            {
                return;
            }

            var childrenItems = _childrenItems.Where(m => m.IsUse).ToList();
            var carousel = VisualHelper.GetChild<Crs_Carousel>(subjectContentControl);
            viewModel.SubPanelCarousel = carousel;

            if (carousel != null)
            {
                for (var pageIndex = 0; pageIndex < carousel.Items.Count; pageIndex++)
                {
                    if (pageIndex < childrenItems.Count && pageIndex == carousel.PageIndex)
                    {
                        var (status, msg) = viewModel.SubjectSelectedItemCheck(selectedItem, childrenItems[pageIndex]);
                        viewModel.SubPanelSubjectSelectedItem = selectedItem;
                        viewModel.SubPanelSubjectSelectedItemIndex = pageIndex + 1;
                        if (!status)
                        {
                            //await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                            //return;

                            viewModel.TherapistScoringStatus = Visibility.Visible;
                            viewModel.NextButtonVisibilityStatus = Visibility.Visible;
                            viewModel.NextButtonIsEnabledStatus = true;
                        }
                        break;
                    }
                }

                //if (carousel.PageIndex < carousel.Items.Count - 1)
                //{
                //    carousel.PageIndex = carousel.PageIndex + 1;

                //    viewModel.TherapistScoringStatus = Visibility.Visible;
                //    viewModel.NextButtonVisibilityStatus = Visibility.Visible;
                //    viewModel.NextButtonIsEnabledStatus = true;

                //    return;
                //}
            }
            else
            {
                var (status, msg) = viewModel.SubjectSelectedItemCheck(selectedItem);
                viewModel.SubPanelSubjectSelectedItem = selectedItem;
                viewModel.SubPanelSubjectSelectedItemIndex = 0;
                if (!status)
                {
                    //await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);

                    viewModel.TherapistScoringStatus = Visibility.Visible;
                    viewModel.NextButtonVisibilityStatus = Visibility.Visible;
                    viewModel.NextButtonIsEnabledStatus = true;

                    //return;
                }
            }

            if (selectedItem.IsLast)
            {
                var msg = "Completed all answers！";
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                return;
            }

            viewModel.TherapistScoringStatus = Visibility.Visible;
            viewModel.NextButtonVisibilityStatus = Visibility.Visible;
            viewModel.NextButtonIsEnabledStatus = true;

            //viewModel.NextCommand.Execute();
            return;
        }
    }
}
