using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private string connString = "Data Source=127.0.0.1;Initial Catalog=QrVisitor;Persist Security Info=True;User ID=sa; Password=mssql_p@ssw0rd!";

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var list = GetVisitor();
            
            DataContext = list;
        }

        private List<Common.Visitor> GetVisitor()
        {
            List<Common.Visitor> list = new List<Common.Visitor>();
            Common.Visitor result = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                var query = @"SELECT * from QRVisitor";

                SqlCommand cmd = new SqlCommand(query, conn);
                var tmp = cmd.ExecuteReader();
                while (tmp.Read())
                {
                    result = new Common.Visitor(Convert.ToInt32(tmp["Idx"]),
                                                tmp["Name"].ToString(),
                                                tmp["PhoneNumber"].ToString(),
                                                Convert.ToDateTime(tmp["VisitDate"]),
                                                Convert.ToChar(tmp["Gender"]));
                    list.Add(result);
                }
            }
            return list;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (isValid())
            {

            }
        }

        private bool isValid()
        {
            throw new NotImplementedException();
        }
    }
}
