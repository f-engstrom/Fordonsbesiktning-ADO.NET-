using System;
using System.Collections.Generic;
using System.Text;

namespace Fordonsbesiktning_ADO.NET_
{

    class Inspections
    {
        public static List<Inspection> AllInspections = new List<Inspection>();

    }

    class Inspection
    {
        public int Id { get; }
        public string RegistrationNumber { get; }
        public DateTime PerformedAt { get; }
        public bool Passed { get; private set; }

        public Inspection(string registrationNumber)
        {
            RegistrationNumber = registrationNumber;
            PerformedAt = DateTime.Now;
        }

        public Inspection(int id, string registrationNumber, DateTime performedAt, bool passed)
        {
            Id = id; RegistrationNumber = registrationNumber;
            PerformedAt = performedAt; 
            Passed = passed;
        }

        public void Pass()
        {
            Passed = true;
        }

        public void Fail()
        {
            Passed = false;
        }
    }
}
