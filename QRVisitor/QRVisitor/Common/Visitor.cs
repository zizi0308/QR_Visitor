using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRVisitor.Common
{
    public class Visitor
    {
        public int Idx { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime VisitDate { get; set; }
        public char Gender { get; set; }

        public Visitor(int idx, string name, string phone, DateTime visitDate, char gender)
        {
            Idx = idx;
            Name = name;
            PhoneNumber = phone;
            VisitDate = visitDate;
            Gender = gender;
        }

        public static string connString = "Data Source=127.0.0.1;Initial Catalog=QrVisitor;Persist Security Info=True;User ID=sa; Password=mssql_p@ssw0rd!";
        
        public static List<Visitor> GetTotal(string start, string end)
        {
            List<Visitor> list = new List<Visitor>();
            Visitor result = null;
            //DB연결
            using (SqlConnection conn = new SqlConnection(connString))
            {
                if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                var query = $@" select * from QrVisitor where VisitDate between '{start}' and '{end}'";

                SqlCommand cmd = new SqlCommand(query, conn);
                var tmp = cmd.ExecuteReader();
                //쿼리 실행
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

        public static List<Visitor> GetMale(string start, string end)
        {
            List<Visitor> list = new List<Visitor>();
            Visitor result = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                var query = $@" select CONVERT(date,VisitDate) as '날짜',Gender,COUNT(VisitDate) as '방문수'
                                    from QrVisitor group by CONVERT(date,VisitDate),Gender 
                                    having CONVERT(date,VisitDate) between '{start}' and '{end}' and Gender = 'M'";

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

        public static List<Visitor> GetFemale(string start, string end)
        {
            List<Visitor> list = new List<Visitor>();
            Visitor result = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                var query = $@" select CONVERT(date,VisitDate) as '날짜',Gender,COUNT(VisitDate) as '방문수'
                                    from QrVisitor group by CONVERT(date,VisitDate),Gender 
                                    having CONVERT(date,VisitDate) between '{start}' and '{end}' and Gender = 'F'";

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
    }
}
