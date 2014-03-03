using System;

using NQuery;

using NQueryDesigner.ViewModels;

namespace NQueryDesigner
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var dataContext = NorthwindDataContext.Instance;
            var viewModel = new DesignerViewModel();
            viewModel.DataContext = dataContext;
            viewModel.Text = @"/*
|| DEMO.SQL
||
|| Gets a list of all employee and assigned territories.
*/

SELECT  TOP 5
        e.LastName + ', ' + e.FirstName Employee,
        COUNT(1) [Territory Count],
        CONCAT(r.RegionDescription) Regions,
        CONCAT(t.TerritoryDescription) Territories

FROM    Region r,
        Employees e,
        Territories t,
        EmployeeTerritories et

WHERE   e.EmployeeID = et.EmployeeID
AND     t.TerritoryID = et.TerritoryID
AND     r.RegionID = t.RegionID

GROUP   BY e.LastName + ', ' + e.FirstName

ORDER   BY COUNT(1) DESC";

            DataContext = viewModel;
        }
    }
}
