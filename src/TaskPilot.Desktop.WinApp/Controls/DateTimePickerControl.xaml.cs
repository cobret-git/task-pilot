using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers.Provider;

namespace TaskPilot.Desktop.WinApp.Controls
{
    /// <summary>
    /// Custom date/time picker control with CalendarView and TimePicker in a unified flyout.
    /// Supports nullable DateTime binding and provides Save/Cancel/Clear actions.
    /// </summary>
    public sealed partial class DateTimePickerControl : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private DateTimeOffset? _tempDate;
        private TimeSpan? _tempTime;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePickerControl"/> class.
        /// </summary>
        public DateTimePickerControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Identifies the <see cref="SelectedDateTime"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register(
                nameof(SelectedDateTime),
                typeof(DateTime?),
                typeof(DateTimePickerControl),
                new PropertyMetadata(null, OnSelectedDateTimeChanged));

        /// <summary>
        /// Identifies the <see cref="PlaceholderText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(PlaceholderText),
                typeof(string),
                typeof(DateTimePickerControl),
                new PropertyMetadata("Select date"));

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(DateTimePickerControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the selected date and time.
        /// </summary>
        public DateTime? SelectedDateTime
        {
            get => (DateTime?)GetValue(SelectedDateTimeProperty);
            set => SetValue(SelectedDateTimeProperty, value);
        }

        /// <summary>
        /// Gets or sets the placeholder text displayed when no date is selected.
        /// </summary>
        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the header text displayed above the control.
        /// </summary>
        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the temporary date selected in the CalendarView before saving.
        /// </summary>
        private DateTimeOffset? TempDate
        {
            get => _tempDate;
            set
            {
                if (_tempDate != value)
                {
                    _tempDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsTempDateNotNull));
                }
            }
        }

        /// <summary>
        /// Gets the temporary time selected in the TimePicker before saving.
        /// </summary>
        private TimeSpan? TempTime
        {
            get => _tempTime;
            set
            {
                if (_tempTime != value)
                {
                    _tempTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsTempTimeNotNull));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the selected date/time is null.
        /// </summary>
        public bool IsDateTimeNull => !SelectedDateTime.HasValue;

        /// <summary>
        /// Gets a value indicating whether the selected date/time is not null.
        /// </summary>
        public bool IsDateTimeNotNull => SelectedDateTime.HasValue;

        /// <summary>
        /// Gets a value indicating whether the time component is not null.
        /// </summary>
        public bool IsTimeNotNull => SelectedDateTime.HasValue && SelectedDateTime.Value.TimeOfDay != TimeSpan.Zero;

        /// <summary>
        /// Gets a value indicating whether the temporary date is not null.
        /// </summary>
        public bool IsTempDateNotNull => TempDate.HasValue;

        /// <summary>
        /// Gets a value indicating whether the temporary time is not null.
        /// </summary>
        public bool IsTempTimeNotNull => TempTime.HasValue;

        /// <summary>
        /// Gets a value indicating whether "No Time" (all-day) is selected.
        /// </summary>
        public bool IsNoTimeSelected { get; private set; }

        /// <summary>
        /// Gets a value indicating whether "With Time" (specific time) is selected.
        /// </summary>
        public bool IsWithTimeSelected { get; private set; }

        /// <summary>
        /// Gets the display text for the date portion.
        /// </summary>
        public string DisplayDateText
        {
            get
            {
                if (!SelectedDateTime.HasValue)
                    return string.Empty;

                return SelectedDateTime.Value.ToString("ddd, MMM d, yyyy", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the display text for the time portion.
        /// </summary>
        public string DisplayTimeText
        {
            get
            {
                if (!SelectedDateTime.HasValue)
                    return string.Empty;

                var time = SelectedDateTime.Value.TimeOfDay;
                if (time == TimeSpan.Zero)
                    return string.Empty;

                return SelectedDateTime.Value.ToString("HH:mm", CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Opened event of the DateTimeFlyout.
        /// Initializes temporary values from the current SelectedDateTime.
        /// </summary>
        private void DateTimeFlyout_Opened(object sender, object e)
        {
            // Initialize temporary values from current selection
            if (SelectedDateTime.HasValue)
            {
                TempDate = new DateTimeOffset(SelectedDateTime.Value);
                TempTime = SelectedDateTime.Value.TimeOfDay;

                // Determine time mode based on whether time is midnight or not
                IsWithTimeSelected = TempTime.HasValue && TempTime.Value != TimeSpan.Zero;
                IsNoTimeSelected = !IsWithTimeSelected;
                OnPropertyChanged(nameof(IsWithTimeSelected));
                OnPropertyChanged(nameof(IsNoTimeSelected));

                // Update CalendarView selection
                DateCalendar.SelectedDates.Clear();
                DateCalendar.SelectedDates.Add(new DateTimeOffset(SelectedDateTime.Value));

                // Update TimePicker
                TimePicker.Time = TempTime.Value;
            }
            else
            {
                TempDate = null;
                TempTime = null;
                IsWithTimeSelected = false;
                IsNoTimeSelected = true;
                OnPropertyChanged(nameof(IsWithTimeSelected));
                OnPropertyChanged(nameof(IsNoTimeSelected));
                DateCalendar.SelectedDates.Clear();
                TimePicker.Time = TimeSpan.Zero;
            }

            if (sender is Flyout flyout && (flyout.Content as FrameworkElement)?.Parent is FlyoutPresenter flyoutPresenter)
            {
                flyoutPresenter.MaxWidth = 330;
                flyoutPresenter.Width = 330;
            }


        }

        /// <summary>
        /// Handles the Closed event of the DateTimeFlyout.
        /// Resets temporary state.
        /// </summary>
        private void DateTimeFlyout_Closed(object sender, object e)
        {
            // Temporary state is already applied or cancelled, no action needed
        }

        /// <summary>
        /// Handles the SelectedDatesChanged event of the CalendarView.
        /// Updates the temporary date and sets default time if needed.
        /// </summary>
        private void DateCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates.Count > 0)
            {
                TempDate = sender.SelectedDates[0];

                // If time is not set, default to 00:00
                if (!TempTime.HasValue)
                {
                    TempTime = TimeSpan.Zero;
                    TimePicker.Time = TimeSpan.Zero;
                }
            }
            else
            {
                TempDate = null;
            }
        }

        /// <summary>
        /// Handles the TimeChanged event of the TimePicker.
        /// Updates the temporary time.
        /// </summary>
        private void TimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            TempTime = e.NewTime;
        }

        /// <summary>
        /// Handles the Click event of the "No Time" toggle button.
        /// Sets time mode to NoTime (all-day) and sets default time to 00:00.
        /// </summary>
        private void NoTimeToggle_Click(object sender, RoutedEventArgs e)
        {
            IsNoTimeSelected = true;
            IsWithTimeSelected = false;
            OnPropertyChanged(nameof(IsNoTimeSelected));
            OnPropertyChanged(nameof(IsWithTimeSelected));
            TempTime = TimeSpan.Zero;
            TimePicker.Time = TimeSpan.Zero;
        }

        /// <summary>
        /// Handles the Click event of the "With Time" toggle button.
        /// Sets time mode to WithTime and enables TimePicker.
        /// </summary>
        private void WithTimeToggle_Click(object sender, RoutedEventArgs e)
        {
            IsNoTimeSelected = false;
            IsWithTimeSelected = true;
            OnPropertyChanged(nameof(IsNoTimeSelected));
            OnPropertyChanged(nameof(IsWithTimeSelected));

            // If time is currently zero, set a reasonable default (e.g., current time or 09:00)
            if (!TempTime.HasValue || TempTime.Value == TimeSpan.Zero)
            {
                // Use a default time like 09:00
                TempTime = new TimeSpan(9, 0, 0);
                TimePicker.Time = TempTime.Value;
            }
        }

        /// <summary>
        /// Handles the Click event of the Cancel button.
        /// Discards temporary changes and closes the flyout.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Discard temporary changes
            TempDate = null;
            TempTime = null;
            DateTimeFlyout.Hide();
        }

        /// <summary>
        /// Handles the Click event of the Save button.
        /// Applies temporary date/time to SelectedDateTime and closes the flyout.
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TempDate.HasValue)
            {
                // Combine date and time
                var dateTime = TempDate.Value.DateTime;
                if (TempTime.HasValue)
                {
                    dateTime = dateTime.Date.Add(TempTime.Value);
                }

                SelectedDateTime = dateTime;
            }
            else
            {
                SelectedDateTime = null;
            }

            DateTimeFlyout.Hide();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the SelectedDateTime dependency property changes.
        /// </summary>
        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateTimePickerControl control)
            {
                control.OnPropertyChanged(nameof(IsDateTimeNull));
                control.OnPropertyChanged(nameof(IsDateTimeNotNull));
                control.OnPropertyChanged(nameof(IsTimeNotNull));
                control.OnPropertyChanged(nameof(DisplayDateText));
                control.OnPropertyChanged(nameof(DisplayTimeText));
            }
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
