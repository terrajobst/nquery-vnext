using System;
using System.Windows.Media;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting.Implementation;

using NQuery.Language.ActiproWpf;
using NQuery.Language.ActiproWpf.Classification;
using NQuery.SampleData;

namespace NQueryViewerActiproWpf
{
    internal sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var language = new NQueryLanguage();
            var classificationTypes = language.GetService<INQueryClassificationTypes>();
            classificationTypes.RegisterAll();

            AmbientHighlightingStyleRegistry.Instance.Register(ClassificationTypes.CompilerError, new HighlightingStyle(Brushes.Blue));

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
