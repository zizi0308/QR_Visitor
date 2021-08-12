using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace QRVisitor
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        
        public TimeSpan ts = new TimeSpan(0, 0, 1);
        public int totalVisit; // 전체 방문자 수
        public int totalM; // 전체 남자 수
        public int totalF; // 전체 여자 수

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            totalVisit = totalM = totalF = 0; //방문자수 초기화
            // 시작일~종료일을 오늘날짜로 초기화
            DtpStartDate.SelectedDateTime = DateTime.Today;
            DtpEndDate.SelectedDateTime = DateTime.Today.AddDays(1);
            //검색
            BtnSearch_Click(sender, e);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 유효성 검사
            if (isValid())
            {
                var startDate = ((DateTime)DtpStartDate.SelectedDateTime - ts).ToString("yyyy-MM-dd hh:mm:ss"); //시작일
                var endDate = ((DateTime)DtpEndDate.SelectedDateTime - ts).ToString("yyyy-MM-dd hh:mm:ss"); //종료일
                var total = Common.Visitor.GetTotal(startDate, endDate);
                var male = Common.Visitor.GetMale(startDate, endDate);
                var female = Common.Visitor.GetFemale(startDate, endDate);

                totalVisit = total.Count();
                totalM = male.Count();
                totalF = female.Count();
                DataContext = total;
                DisplayChart(total);
            }
            lbltotal.Content = $"총 방문객 수 : {totalVisit} 명 \n남자 : {totalM} 명 | 여자 : {totalF} 명";
            totalVisit = totalM = totalF = 0;
        }

        public void DisplayChart(List<Common.Visitor> list)
        {
            //범례 위치 설정
            chart.LegendLocation = LiveCharts.LegendLocation.Top;

            //세로 눈금 값 설정
            chart.AxisY.Add(new LiveCharts.Wpf.Axis { MinValue = 0, MaxValue = 10 });

            //가로 눈금 값 설정
            //chart.AxisX.Add(new LiveCharts.Wpf.Axis { Labels = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" } });
            //chart.AxisX.First().Labels = list.Select(a => a.VisitDate.ToString("yy-MM-dd")).ToList();
            chart.AxisX.Add(new LiveCharts.Wpf.Axis { Labels = list.Select(a => a.VisitDate.ToString("yy-MM-dd")).ToList() });

            //모든 항목 지우기
            chart.Series.Clear();

            //항목 추가
            chart.Series.Add(new LiveCharts.Wpf.LineSeries()
            {
                Title = "남자",
                Stroke = new SolidColorBrush(Colors.Blue),
                Values = new LiveCharts.ChartValues<double>(new List<double> { 7, 2, 3, 4, 5, 600, 700, 800, 900, 90, 211, 220 })
            });
            chart.Series.Add(new LiveCharts.Wpf.LineSeries()
            {
                Title = "여자",
                Stroke = new SolidColorBrush(Colors.Red),
                Values = new LiveCharts.ChartValues<double>(new List<double> { 7, 2, 1, 140, 50, 60, 70, 80, 90, 100, 111, 120 })
            });
        }

        private bool isValid()
        {
            var result = true;
            // 값이 있는지
            if (DtpStartDate.SelectedDateTime == null | DtpEndDate.SelectedDateTime == null)
            {
                this.ShowMessageAsync("검색", "검색할 일자를 선택하세요");
                result = false;
            }
            // 시작일이 종료일보다 더 뒤인지
            if (DtpEndDate.SelectedDateTime < DtpStartDate.SelectedDateTime)
            {
                this.ShowMessageAsync("검색", "시작일자가 종료일자보다 최신일 수 없습니다");
                result = false;
            }
            // 해당되는게 없으면 참 반환
            return result;
        }
    }
}
