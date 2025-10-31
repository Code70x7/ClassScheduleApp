namespace ClassScheduleApp.Views;

public partial class SafeHomePage : ContentPage
{
    public SafeHomePage() => InitializeComponent();

    private async void OpenTerms_Clicked(object? sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new TermsPage()); }
        catch (Exception ex) { await DisplayAlert("Navigation", ex.Message, "OK"); }
    }
}
