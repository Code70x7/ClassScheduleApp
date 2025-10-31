using System.Collections.ObjectModel;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.ViewModels
{
    public class TermsViewModel : BaseViewModel
    {
        private readonly AppDatabase _db;
        public ObservableCollection<Term> Terms { get; } = new();

        public TermsViewModel(AppDatabase db)
        {
            _db = db;
        }

        public async Task LoadAsync()
        {
            Terms.Clear();
            var terms = await _db.GetTermsAsync();
            foreach (var t in terms.OrderBy(t => t.StartDate))
                Terms.Add(t);
        }

        public async Task AddTermAsync()
        {
            var t = new Term
            {
                Title = "New Term",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6)
            };
            await _db.SaveTermAsync(t);
            await LoadAsync();
        }

        public Task SaveTermAsync(Term term) => _db.SaveTermAsync(term);
        public Task DeleteTermAsync(Term term) => _db.DeleteTermAsync(term);
    }
}
