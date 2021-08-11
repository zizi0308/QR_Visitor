using System;
using System.Collections.Generic;
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
    }
}
