using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

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
            InitConnectMqttBroker();
        }

        MqttClient client;
        public TimeSpan ts = new TimeSpan(0, 0, 1);
        public int totalVisit; // 전체 방문자 수
        public int totalM; // 전체 남자 수
        public int totalF; // 전체 여자 수

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            totalVisit = totalM = totalF = 0; //방문자수 초기화
            // 시작일~종료일을 오늘날짜로 초기화
            DtpStartDate.SelectedDateTime = DateTime.Today;             // 처음 로드는 오늘 화면만 보여줌
            DtpEndDate.SelectedDateTime = DateTime.Today.AddDays(1);
            //검색
            BtnSearch_Click(sender, e);
        }

        /* MQTT서버 연결 부분 */
        private void InitConnectMqttBroker()
        {
            var brokerAddress = IPAddress.Parse("210.119.12.50");
            client = new MqttClient(brokerAddress);     // IP주소를 받아옴
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;     // MQTT 메세지 받았을때 처리
            client.Connect("Monitor");
            client.Subscribe(new string[] { "QR_Reader/data/" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });    // 데이터 받는거 최대한번
            MessageBox.Show("접속완료");
        }

        Dictionary<string, string> currentData = new Dictionary<string, string>();

        /* 받은 메세지를 Json형태로 바꿔주고 딕셔너리 형태로 바꿔서 currentData에 넣어줌 */
        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)    
        {
            var message = Encoding.UTF8.GetString(e.Message);
            currentData = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            UpdateData(currentData);
        }


        /* 데이터를 받을때마다 visitor 객체 생성 */
        private void UpdateData(Dictionary<string, string> currentData)
        {
            Common.Visitor visitor = new Common.Visitor(
                currentData["Name"].ToString(),
                currentData["PhoneNumber"].ToString(),
                Convert.ToChar(currentData["Gender"]),
                Convert.ToDateTime(currentData["VisitDate"])
                );
            Common.Visitor.UpdateDBData(visitor);       // 생성객체를 DB에 저장
            LoadData();                                 // 저장된 데이터를 화면에 뿌림
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            totalVisit = totalM = totalF = 0;
            // 유효성 검사
            if (isValid())
            {
                LoadData();
                
            }
        }

        private void LoadData()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate // 스레드 문제가 생길 경우에는 이 코드를 써주면 됨
            {
                /* 기간을 정해서 데이터를 뿌려줌 */
                var startDate = ((DateTime)DtpStartDate.SelectedDateTime - ts).ToString("yyyy-MM-dd hh:mm:ss"); //시작일
                var endDate = ((DateTime)DtpEndDate.SelectedDateTime - ts).ToString("yyyy-MM-dd hh:mm:ss");     //종료일
                /* Get함수를 써서 해당 데이터를 로드 */
                var data = Common.Visitor.GetData(startDate, endDate);
                var male = Common.Visitor.GetMale(startDate, endDate);
                var female = Common.Visitor.GetFemale(startDate, endDate);
                var total = Common.Visitor.GetTotal(startDate, endDate);

                totalVisit = data.Count();
                totalM = data.Where(a => a.Gender.Equals('M')).Count();
                totalF = data.Where(a => a.Gender.Equals('F')).Count();
                DataContext = data;

                DisplayChart(total, male, female);

                lbltotal.Content = $"총 방문객 수 : {totalVisit} 명 \n남자 : {totalM} 명 | 여자 : {totalF} 명";
            }));
           
        }

        /* LiveChart 부분 */
        public void DisplayChart(List<Common.Visitor> total, List<Common.Visitor> male, List<Common.Visitor> female)
        {
            string[] dates = total.Select(a => a.VisitDate.ToString("yy-MM-dd")).ToArray();
            int[] totals = total.Select(a => (int)a.Person).ToArray();
            int[] males = male.Select(a => (int)a.Person).ToArray();
            int[] females = female.Select(a => (int)a.Person).ToArray();

            //범례 위치 설정
            chart.LegendLocation = LiveCharts.LegendLocation.Top;

            chart.AxisX.Clear();
            chart.AxisY.Clear();

            //세로 눈금 값 설정
            chart.AxisY.Add(new LiveCharts.Wpf.Axis { MinValue = 0, MaxValue = 10 });

            //가로 눈금 값 설정
            chart.AxisX.Add(new LiveCharts.Wpf.Axis { Labels = dates });

            //모든 항목 지우기
            chart.Series.Clear();

            chart.Series.Add(new LiveCharts.Wpf.LineSeries()
            {
                Title = "총합",
                Stroke = new SolidColorBrush(Colors.Green),
                Values = new LiveCharts.ChartValues<int>(totals)
            });
            //항목 추가
            /*chart.Series.Add(new LiveCharts.Wpf.LineSeries()
            {
                Title = "남자",
                Stroke = new SolidColorBrush(Colors.Blue),
                Values = new LiveCharts.ChartValues<int>(males)
            });
            chart.Series.Add(new LiveCharts.Wpf.LineSeries()
            {
                Title = "여자",
                Stroke = new SolidColorBrush(Colors.Red),
                Values = new LiveCharts.ChartValues<int>(females)
            });*/
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

        private void MetroWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            client.Disconnect();
        }
    }
}
