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

        public int Person { get; set; }

        public Visitor(int idx, string name, string phone, DateTime visitDate, char gender)
        {
            Idx = idx;
            Name = name;
            PhoneNumber = phone;
            VisitDate = visitDate;
            Gender = gender;
        }
        public Visitor(string name, string phone, char gender, DateTime visitDate)
        {
            Name = name;
            PhoneNumber = phone;
            VisitDate = visitDate;
            Gender = gender;
        }
        public Visitor(DateTime visitDate, char gender, int person)
        {
            VisitDate = visitDate;
            Gender = gender;
            Person = person;
        }
        public Visitor(DateTime visitDate, int person)
        {
            VisitDate = visitDate;
            Person = person;
        }


        public static string connString = "Data Source=127.0.0.1;Initial Catalog=QrVisitor;Persist Security Info=True;User ID=sa; Password=msspl_p@ssw0rd!";

        public static void UpdateData(Visitor visitor)
        {
            List<Visitor> list = new List<Visitor>();
            Visitor result = null;
            //DB연결
            using (SqlConnection conn = new SqlConnection(connString))
            {
                if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                var query = $@"INSERT INTO dbo.QRVisitor
                                            (Name
                                            ,PhoneNumber
                                            ,VisitDate
                                            ,Gender)
                                        VALUES
                                            ('{visitor.Name}'
                                            ,'{visitor.PhoneNumber}'
                                            ,'{visitor.VisitDate.ToString("yyyy-MM-dd HH:mm:ss")}'
                                            ,'{visitor.Gender}')";

                SqlCommand cmd = new SqlCommand(query, conn);
                var tmp = cmd.ExecuteReader();
                //쿼리 실행
                tmp.Read();
            }
        }

        public static List<Visitor> GetData(string start, string end)
        {
            List<Visitor> list = new List<Visitor>();
            Visitor result = null;
            //DB연결
            using (SqlConnection conn = new SqlConnection(connString))
            {
                if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                var query = $@" Select * from QrVisitor where VisitDate between '{start}' and '{end}'";

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

        public static List<Visitor> GetTotal(string start, string end)
        {
            List<Visitor> list = new List<Visitor>();
            Visitor result = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                if (conn.State == System.Data.ConnectionState.Closed) conn.Open();


                var query = $@"select convert(varchar(10),VisitDate,120) as VisitDate,COUNT(VisitDate) as Person
                                  from QrVisitor
                                  where VisitDate
                                  between '{start}' and '{end}'
                                  group by convert(varchar(10),VisitDate,120) ";

                SqlCommand cmd = new SqlCommand(query, conn);
                var tmp = cmd.ExecuteReader();
                while (tmp.Read())
                {
                    result = new Common.Visitor(Convert.ToDateTime(tmp["VisitDate"]),
                                                Convert.ToInt32(tmp["Person"]));
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


                var query = $@"select convert(varchar(10),VisitDate,120) as VisitDate,Gender,COUNT(Gender) as Person
                                  from QrVisitor
                                  where VisitDate
                                  between '{start}' and '{end}'
                                  group by convert(varchar(10),VisitDate,120),Gender having Gender ='M' ";

                SqlCommand cmd = new SqlCommand(query, conn);
                var tmp = cmd.ExecuteReader();
                while (tmp.Read())
                {
                    result = new Common.Visitor(Convert.ToDateTime(tmp["VisitDate"]),
                                                Convert.ToChar(tmp["Gender"]),
                                                Convert.ToInt32(tmp["Person"]));
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

                var query = $@"select convert(varchar(10),VisitDate,120) as VisitDate,Gender,COUNT(Gender) as Person
                                  from QrVisitor
                                  where VisitDate
                                  between '{start}' and '{end}'
                                  group by convert(varchar(10),VisitDate,120),Gender having Gender ='F' ";

                SqlCommand cmd = new SqlCommand(query, conn);
                var tmp = cmd.ExecuteReader();
                while (tmp.Read())
                {
                    result = new Common.Visitor(Convert.ToDateTime(tmp["VisitDate"]),
                                                Convert.ToChar(tmp["Gender"]),
                                                Convert.ToInt32(tmp["Person"]));
                    list.Add(result);
                }
            }
            return list;
        }
    }
}
