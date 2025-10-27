using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9, "DatePicker Does Not Update Its Format When the Culture Is Changed at Runtime",
	PlatformAffected.All)]
public partial class Issue9 : ContentPage, INotifyPropertyChanged
{
	private string _currentCulture = string.Empty;
	private DateTime _testDate = DateTime.Today;
	private string _testResults = "No tests run yet.";

	public Issue9()
	{
		InitializeComponent();
		BindingContext = this;
		
		CurrentCulture = CultureInfo.CurrentCulture.DisplayName;
		ChangeCultureCommand = new Command<string>(async (culture) => await ChangeCulture(culture));
	}

	public string CurrentCulture
	{
		get => _currentCulture;
		set
		{
			_currentCulture = value;
			OnPropertyChanged();
		}
	}

	public DateTime TestDate
	{
		get => _testDate;
		set
		{
			_testDate = value;
			OnPropertyChanged();
		}
	}

	public string TestResults
	{
		get => _testResults;
		set
		{
			_testResults = value;
			OnPropertyChanged();
		}
	}

	public ICommand ChangeCultureCommand { get; }

	private async Task ChangeCulture(string cultureName)
	{
		try
		{
			var previousFormat = GetCurrentDateFormat();
			
			// Change the culture
			var culture = new CultureInfo(cultureName);
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;
			
			CurrentCulture = culture.DisplayName;
			
			// Wait a moment for the culture change to propagate
			await Task.Delay(100);
			
			var newFormat = GetCurrentDateFormat();
			
			TestResults = $"Culture changed to {cultureName}\n" +
			             $"Previous format: {previousFormat}\n" +
			             $"New format: {newFormat}\n" +
			             $"Date example: {TestDate.ToString("d")}\n" +
			             $"Test completed at: {DateTime.Now:HH:mm:ss}";
		}
		catch (Exception ex)
		{
			TestResults = $"Error changing culture: {ex.Message}";
		}
	}

	private string GetCurrentDateFormat()
	{
		return TestDate.ToString("d", CultureInfo.CurrentCulture);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}