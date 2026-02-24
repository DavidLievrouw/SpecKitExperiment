using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ModalCalendarNotification.UI.Features.CalendarSelection;

public partial class CalendarListDialog : Window
{
    private readonly CalendarListViewModel _viewModel;

    public CalendarListDialog(CalendarListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Subscribe to IsSaved to close dialog when selections are saved
        if (_viewModel is ObservableObject observableObject)
        {
            observableObject.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CalendarListViewModel.IsSaved) && _viewModel.IsSaved)
                {
                    this.DialogResult = true;
                    this.Close();
                }
            };
        }
    }
}
