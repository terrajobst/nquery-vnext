using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Parsing.Implementation;
using NQuery.SampleData;

namespace NQueryViewerActiproWpf
{
    internal sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            AmbientParseRequestDispatcherProvider.Dispatcher = new ThreadedParseRequestDispatcher();

            var language = new NQueryLanguage();
            var classificationTypes = language.GetService<INQueryClassificationTypes>();
            classificationTypes.RegisterAll();

            _syntaxEditor.Document = new NQueryDocument { DataContext = NorthwindDataContext.Create() };
            _syntaxEditor.Text = @"/*
|| DEMO.SQL
||
|| Gets a list of all employees and assigned territories.
*/

SELECT	TOP 5
		e.LastName + ', ' + e.FirstName Employee,
		COUNT(*) [Territory Count],
		CONCAT(r.RegionDescription) Regions,
		CONCAT(t.TerritoryDescription) Territories

FROM	Region r,
		Employees e,
		Territories t,
		EmployeeTerritories et
	
WHERE	e.EmployeeID = et.EmployeeID
AND		t.TerritoryID = et.TerritoryID
AND     r.RegionID = t.RegionID

GROUP   BY e.LastName + ', ' + e.FirstName";
        }
    }
}
