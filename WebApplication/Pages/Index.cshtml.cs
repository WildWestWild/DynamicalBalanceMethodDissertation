using System;
using System.Data;
using System.Globalization;
using System.Linq;
using FusionCharts.DataEngine;
using FusionCharts.Visualization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocketCreatingLib;
using WebApplication.ParserArea;

namespace WebApplication.Pages
{
    public class Index : PageModel
    {
        public string ChartJson { get; internal set; }
        
        public string SplineJson { get; internal set; }
        
        public string CombinationChart { get; internal set; }

        private const string ChartLeftLineText = "Duration";

        private const string ChartBottomLineText = "Nodes";
        
        private const string SplineLeftLineText = "Load";
        
        private const string SplineBottomLineText = "Time";

        private const string PathArrayOfRequest = "ParserArea/ArrayOfRequest.json";
        
        private const string PathFirstStartApplication = "ParserArea/FirstStartApplication.json";

        private const int Width = 1000;

        private const int Height = 400;
        
        public void OnGet()
        {
            CreateColumnDiagram();
            CreateCombinationChart();
        }
        
        
        private void CreateColumnDiagram()
        {
            // create data table to store data
            DataTable ChartData = new DataTable();
            // Add columns to data table
            ChartData.Columns.Add(ChartBottomLineText, typeof(String));
            ChartData.Columns.Add(ChartLeftLineText, typeof(Int64));
            // Add rows to data table
            
            
            var arrayOfNodes = TableWeightHandler.WeightTable;
            if (arrayOfNodes != null)
            {
                foreach (var node in arrayOfNodes)
                {
                    ChartData.Rows.Add("Port - " + node.Port + "( Weight - " + node.Weight + " )", node.Duration); 
                }
            }
            else
            {
                ChartData.Rows.Add("Port - 3011", 0);
                ChartData.Rows.Add("Port - 3012", 0);
                ChartData.Rows.Add("Port - 3013", 0);
                ChartData.Rows.Add("Port - 3014", 0);
            }
            // Create static source with this data table
            StaticSource source = new StaticSource(ChartData);
            // Create instance of DataModel class
            DataModel model = new DataModel();
            // Add DataSource to the DataModel
            model.DataSources.Add(source);
            // Instantiate Column Chart
            Charts.ColumnChart column = new Charts.ColumnChart("first_chart");
            // Set Chart's width and height
            column.Width.Pixel(Width);
            column.Height.Pixel(Height);
            // Set DataModel instance as the data source of the chart
            column.Data.Source = model;
           
            column.Legend.Show = false;
            // set XAxis Text
            column.XAxis.Text = ChartBottomLineText;
            // Set YAxis title
            column.YAxis.Text = ChartLeftLineText;
            // set chart theme
            column.ThemeName = FusionChartsTheme.ThemeName.FUSION;
            // set chart rendering json
            ChartJson = column.Render();
        }

        private void CreateLineChart()
        {
            DataTable ChartData = new DataTable();
            ChartData.Columns.Add(SplineBottomLineText, typeof(String));
            ChartData.Columns.Add(SplineLeftLineText, typeof(Int32));
            
            var arrayOfRequests = TableWeightHandler.ArrayOfRequests?.OrderBy(r=> r.TimeMilliseconds);
            if (arrayOfRequests != null)
            {
                foreach (var request in arrayOfRequests)
                {
                    ChartData.Rows.Add(request.TimeMilliseconds, request.Load);
                } 
            }
            else
            {
                ChartData.Rows.Add("0", 0);
            }
            StaticSource source = new StaticSource(ChartData);
            DataModel model = new DataModel();
            model.DataSources.Add(source);

            Charts.LineChart spline = new Charts.LineChart("line_chart_db");

            spline.ThemeName = FusionChartsTheme.ThemeName.FUSION;
            spline.Width.Pixel(Width);
            spline.Height.Pixel(Height);

            spline.Data.Source = model;
            spline.XAxis.Text = SplineLeftLineText;
            spline.YAxis.Text = SplineBottomLineText;

            spline.Legend.Show = false;
            SplineJson = spline.Render();
        }

        public void CreateCombinationChart()
        {
            // initialixe DataModel object
            string pathFile = String.Empty;
            DataModel model = new DataModel();
            // Create object of JsonFileSource. Provide file path as constructor parameter
            //https://raw.githubusercontent.com/poushali-guha-12/SampleData/master/mscombi3d.json
            
            if (TableWeightHandler.ArrayOfRequests?.Count > 0)
            {
                JSONHandler.AddArrayOfJSONRequests(TableWeightHandler.ArrayOfRequests);
                pathFile = PathArrayOfRequest;
            }
            else
            {
                pathFile = PathFirstStartApplication;
            }

            JsonFileSource jsonFileSource = new JsonFileSource(pathFile);
            // Add json source in datasources store of model
            model.DataSources.Add(jsonFileSource);
            // initialize combination chart object
            Charts.CombinationChart combiChart = new Charts.CombinationChart("mscombi3d");
            // Set threeD
            combiChart.ThreeD = true;
            // set model as data source
            combiChart.Data.Source = model;
            // provide field name, which should be rendered as line column
            combiChart.Data.ColumnPlots("Load");
            // provide field name, which should be rendered as area plot
            combiChart.Data.AreaPlots("MiddleLoadLine");
            // Set XAxis caption
            combiChart.XAxis.Text = SplineBottomLineText;
            // Set YAxis caption
            combiChart.PrimaryYAxis.Text = SplineLeftLineText;
            // set width, height
            combiChart.Width.Pixel(Width);
            combiChart.Height.Pixel(Height);
            // set theme
            combiChart.ThemeName = FusionChartsTheme.ThemeName.FUSION;
            // Render chart
            CombinationChart = combiChart.Render();
        }

        
    }
}