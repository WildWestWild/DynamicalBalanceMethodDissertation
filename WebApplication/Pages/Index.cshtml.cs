using System;
using System.Data;
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
            DataTable ChartData = new DataTable();
            ChartData.Columns.Add(ChartBottomLineText, typeof(String));
            ChartData.Columns.Add(ChartLeftLineText, typeof(Int64));
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
            StaticSource source = new StaticSource(ChartData);
            DataModel model = new DataModel();
            model.DataSources.Add(source);
            Charts.ColumnChart column = new Charts.ColumnChart("first_chart");
            column.Width.Pixel(Width);
            column.Height.Pixel(Height);
            column.Data.Source = model;
            column.Legend.Show = false;
            column.XAxis.Text = ChartBottomLineText;
            column.YAxis.Text = ChartLeftLineText;
            column.ThemeName = FusionChartsTheme.ThemeName.FUSION;
            ChartJson = column.Render();
        }
        
        public void CreateCombinationChart()
        {
            string pathFile = String.Empty;
            DataModel model = new DataModel();
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
            model.DataSources.Add(jsonFileSource);
            Charts.CombinationChart combiChart = new Charts.CombinationChart("mscombi3d");
            combiChart.ThreeD = true;
            combiChart.Data.Source = model;
            combiChart.Data.ColumnPlots("Load");
            combiChart.Data.AreaPlots("MiddleLoadLine");
            combiChart.XAxis.Text = SplineBottomLineText;
            combiChart.PrimaryYAxis.Text = SplineLeftLineText;
            combiChart.Width.Pixel(Width);
            combiChart.Height.Pixel(Height);
            combiChart.ThemeName = FusionChartsTheme.ThemeName.FUSION;
            CombinationChart = combiChart.Render();
        }
    }
}