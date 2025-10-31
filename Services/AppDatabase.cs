// File: Services/AppDatabase.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using ClassScheduleApp.Models;
using ClassScheduleApp.Helpers;

namespace ClassScheduleApp.Services
{
    public class AppDatabase
    {
        private readonly SQLiteAsyncConnection _conn;

        // Init guard (prevents "table not found" crashes on fresh install)
        private bool _didInit;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public AppDatabase(SQLiteAsyncConnection conn)
        {
            _conn = conn;
        }

        // ---------- Helpers ----------
        private static string GetTableName<T>()
        {
            // Use [Table("Name")] if present; else type name
            var attr = typeof(T).GetCustomAttribute<TableAttribute>();
            return string.IsNullOrWhiteSpace(attr?.Name) ? typeof(T).Name : attr!.Name!;
        }

        private static bool HasCol(List<SQLiteConnection.ColumnInfo> cols, string name) =>
            cols?.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)) == true;

        private async Task EnsureInitializedAsync()
        {
            if (_didInit) return;
            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_didInit) return;
                await InitializeAsync().ConfigureAwait(false);
                _didInit = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        // ---------- Initialize ----------
        public async Task InitializeAsync()
        {
            await _conn.CreateTableAsync<Term>();
            await _conn.CreateTableAsync<Course>();
            await _conn.CreateTableAsync<Assessment>();
            await _conn.CreateTableAsync<TodoItem>();
            await _conn.CreateTableAsync<UserAccount>();

            // Safe migration: add Notes to Assessments if missing
            try
            {
                var assessTable = GetTableName<Assessment>(); // respects [Table("Assessments")]
                await _conn.ExecuteAsync($"ALTER TABLE {assessTable} ADD COLUMN Notes TEXT;");
            }
            catch
            {
                // ignore if already exists
            }

            // Helpful indexes (use mapped names so singular/plural never mismatches)
            try
            {
                var termTable = GetTableName<Term>();
                var courseTable = GetTableName<Course>();
                var assessTable = GetTableName<Assessment>();
                var todoTable = GetTableName<TodoItem>();

                await _conn.ExecuteAsync($"CREATE INDEX IF NOT EXISTS idx_{termTable}_title   ON {termTable}(Title);");
                await _conn.ExecuteAsync($"CREATE INDEX IF NOT EXISTS idx_{courseTable}_title ON {courseTable}(Title);");
                await _conn.ExecuteAsync($"CREATE INDEX IF NOT EXISTS idx_{assessTable}_title ON {assessTable}(Title);");
                await _conn.ExecuteAsync($"CREATE INDEX IF NOT EXISTS idx_{todoTable}_title   ON {todoTable}(Title);");

                await _conn.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS idx_user_email ON UserAccount(Email);");
            }
            catch
            {
                // non-fatal
            }
        }

        // ---------- Users (for login) ----------
        public async Task<UserAccount?> GetUserByEmailAsync(string email)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<UserAccount>().Where(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<int> SaveUserAsync(UserAccount u)
        {
            await EnsureInitializedAsync();
            return u.Id == 0 ? await _conn.InsertAsync(u) : await _conn.UpdateAsync(u);
        }

        public async Task<bool> CreateUserAsync(string email, string password)
        {
            await EnsureInitializedAsync();

            email = (email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            var existing = await _conn.Table<UserAccount>()
                                      .Where(u => u.Email == email)
                                      .FirstOrDefaultAsync();
            if (existing != null) return false;

            var hashed = PasswordHasher.HashNew(password); // PBKDF2 "salt:hash"
            var user = new UserAccount { Email = email, PasswordHash = hashed };
            await _conn.InsertAsync(user);
            return true;
        }

        // Accept legacy plaintext once (upgrade), otherwise verify hash
        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            await EnsureInitializedAsync();

            email = (email ?? "").Trim().ToLowerInvariant();
            var user = await _conn.Table<UserAccount>().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user is null) return false;

            var stored = user.PasswordHash ?? "";

            // Legacy plaintext row support (upgrade on first successful login)
            if (!string.IsNullOrEmpty(stored) && !stored.Contains(':'))
            {
                var ok = stored == password;
                if (ok)
                {
                    user.PasswordHash = PasswordHasher.HashNew(password);
                    await _conn.UpdateAsync(user);
                }
                return ok;
            }
            return PasswordHasher.Verify(password, stored);
        }

        // ---------- Terms ----------
        public async Task<List<Term>> GetTermsAsync()
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Term>().OrderBy(t => t.StartDate).ToListAsync();
        }

        public async Task<Term?> GetTermByIdAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Term>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTermAsync(Term t)
        {
            await EnsureInitializedAsync();
            return t.Id == 0 ? await _conn.InsertAsync(t) : await _conn.UpdateAsync(t);
        }

        public async Task<int> DeleteTermAsync(Term t)
        {
            await EnsureInitializedAsync();
            return await _conn.DeleteAsync(t);
        }

        public async Task<int> DeleteTermAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Term>().DeleteAsync(x => x.Id == id);
        }

        // ---------- Courses ----------
        public async Task<List<Course>> GetCoursesByTermAsync(int termId)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Course>()
                              .Where(c => c.TermId == termId)
                              .OrderBy(c => c.StartDate)
                              .ToListAsync();
        }

        public async Task<int> SaveCourseAsync(Course c)
        {
            await EnsureInitializedAsync();
            return c.Id == 0 ? await _conn.InsertAsync(c) : await _conn.UpdateAsync(c);
        }

        public async Task<int> DeleteCourseAsync(Course c)
        {
            await EnsureInitializedAsync();
            return await _conn.DeleteAsync(c);
        }

        public async Task<int> DeleteCourseAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Course>().DeleteAsync(x => x.Id == id);
        }

        // ---------- Assessments ----------
        public async Task<List<Assessment>> GetAssessmentsByCourseAsync(int courseId)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Assessment>()
                              .Where(a => a.CourseId == courseId)
                              .OrderBy(a => a.EndDate)
                              .ToListAsync();
        }

        public async Task<int> SaveAssessmentAsync(Assessment a)
        {
            await EnsureInitializedAsync();
            return a.Id == 0 ? await _conn.InsertAsync(a) : await _conn.UpdateAsync(a);
        }

        public async Task<int> DeleteAssessmentAsync(Assessment a)
        {
            await EnsureInitializedAsync();
            return await _conn.DeleteAsync(a);
        }

        public async Task<int> DeleteAssessmentAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Assessment>().DeleteAsync(x => x.Id == id);
        }

        // ---------- To-Dos ----------
        public async Task<List<TodoItem>> GetTodosByCourseAsync(int courseId)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<TodoItem>()
                              .Where(t => t.CourseId == courseId)
                              .OrderBy(t => t.DueDate)
                              .ToListAsync();
        }

        // Back-compat wrapper (so old code keeps working)
        public Task<int> UpsertTodoAsync(TodoItem t) => SaveTodoAsync(t);

        public async Task<int> SaveTodoAsync(TodoItem t)
        {
            await EnsureInitializedAsync();
            return t.Id == 0 ? await _conn.InsertAsync(t) : await _conn.UpdateAsync(t);
        }

        public async Task<int> DeleteTodoAsync(TodoItem t)
        {
            await EnsureInitializedAsync();
            return await _conn.DeleteAsync(t);
        }

        public async Task<int> DeleteTodoAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _conn.Table<TodoItem>().DeleteAsync(x => x.Id == id);
        }
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Course>().ToListAsync();
        }

        public async Task<List<Assessment>> GetAllAssessmentsAsync()
        {
            await EnsureInitializedAsync();
            return await _conn.Table<Assessment>().ToListAsync();
        }

        public async Task<List<TodoItem>> GetAllTodosAsync()
        {
            await EnsureInitializedAsync();
            return await _conn.Table<TodoItem>().ToListAsync();
        }

        // ---------- Search ----------
        public async Task<SearchResult> SearchAsync(string text)
        {
            await EnsureInitializedAsync();

            var q = (text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return new SearchResult();

            var like = $"%{q.ToLowerInvariant()}%";

            // Resolve actual table names from model attributes
            var termTable = GetTableName<Term>();
            var courseTable = GetTableName<Course>();
            var assessTable = GetTableName<Assessment>();
            var todoTable = GetTableName<TodoItem>();

            // Column detection (Notes may or may not exist depending on migration)
            var termCols = await _conn.GetTableInfoAsync(termTable);
            var courseCols = await _conn.GetTableInfoAsync(courseTable);
            var assessCols = await _conn.GetTableInfoAsync(assessTable);
            var todoCols = await _conn.GetTableInfoAsync(todoTable);

            var courseHasNotes = HasCol(courseCols, "Notes");
            var assessHasNotes = HasCol(assessCols, "Notes");
            var todoHasNotes = HasCol(todoCols, "Notes");

            // Build WHEREs (avoid LIKE on enum columns such as Assessment.Type)
            var termWhere = "LOWER(COALESCE(Title,'')) LIKE ?";
            var courseWhere = "LOWER(COALESCE(Title,'')) LIKE ?" + (courseHasNotes ? " OR LOWER(COALESCE(Notes,'')) LIKE ?" : "");
            var assessWhere = "LOWER(COALESCE(Title,'')) LIKE ?" + (assessHasNotes ? " OR LOWER(COALESCE(Notes,'')) LIKE ?" : "");
            var todoWhere = "LOWER(COALESCE(Title,'')) LIKE ?" + (todoHasNotes ? " OR LOWER(COALESCE(Notes,'')) LIKE ?" : "");

            object[] Params(string where) =>
                where.Contains(" OR ", StringComparison.Ordinal) ? new object[] { like, like } : new object[] { like };

            var termsTask = _conn.QueryAsync<Term>(
                $"SELECT * FROM {termTable} WHERE {termWhere} ORDER BY StartDate;", like);

            var coursesTask = _conn.QueryAsync<Course>(
                $"SELECT * FROM {courseTable} WHERE {courseWhere} ORDER BY StartDate;", Params(courseWhere));

            var assessmentsTask = _conn.QueryAsync<Assessment>(
                $"SELECT * FROM {assessTable} WHERE {assessWhere} ORDER BY EndDate;", Params(assessWhere));

            var todosTask = _conn.QueryAsync<TodoItem>(
                $"SELECT * FROM {todoTable} WHERE {todoWhere} ORDER BY DueDate;", Params(todoWhere));

            await Task.WhenAll(termsTask, coursesTask, assessmentsTask, todosTask);

            return new SearchResult
            {
                Terms = termsTask.Result,
                Courses = coursesTask.Result,
                Assessments = assessmentsTask.Result,
                Todos = todosTask.Result
            };
        }
    }
}
