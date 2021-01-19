using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using static System.Console;

namespace Fordonsbesiktning_ADO.NET_
{
    class Program
    {
        public static string connectionString = @"Data Source=.;Initial Catalog=BilProvning;Integrated Security=True";

        static void Main(string[] args)
        {
            Program.Menu();
        }
        static void Menu()
        {
            bool shouldNotExit = true;

            while (shouldNotExit)
            {

                Clear();


                WriteLine("1. Ny Reservation");
                WriteLine("2. Lista Reservationer");
                WriteLine("3. Utför besiktning");
                WriteLine("4. Lista besiktningar");
                WriteLine("5. Exit");

                WriteLine("\n (S)kapa tabeller");

                ConsoleKeyInfo keyPressed = ReadKey(true);


                switch (keyPressed.Key)
                {

                    case ConsoleKey.D1:

                        Clear();
                        CreateReservation();

                        break;

                    case ConsoleKey.D2:

                        Clear();
                        ListReservations();

                        break;

                    case ConsoleKey.D3:

                        Clear();
                        PerformInspection();

                        break;

                    case ConsoleKey.D4:

                        Clear();
                        ListInspections();

                        break;

                    case ConsoleKey.D5:

                        shouldNotExit = false;

                        break;
                    case ConsoleKey.S:

                        Clear();

                        CreateTables();

                        break;

                }

            }
        }




        private static void PerformInspection()
        {
            Reservations.AllReservations = GetReservationsFromDatabase();
          
            bool reservationExists = false;

            WriteLine("Registreringsnummer: ");
            string registrationNumber = ReadLine();

            //List<Reservation> correctReservation = Reservations.AllReservations.Where(r => r.RegistrationNumber.Normalize() == registrationNumber).ToList();

            //foreach (var reservation in correctReservation)
            //{
                

            //}

           // string normalizedRegistrationNumber;

            foreach (var reservation in Reservations.AllReservations)
            {
               

                if (reservation.RegistrationNumber == registrationNumber)
                {
                    reservationExists = true;

                }


            }

            if (reservationExists)
            {
                bool incorrectKey = true;

                Console.WriteLine("Fordonet godkänt? (J)a eller (N)ej");

                ConsoleKeyInfo inputKey;
                do
                {

                    inputKey = ReadKey(true);

                    incorrectKey = !(inputKey.Key == ConsoleKey.J || inputKey.Key == ConsoleKey.N);



                } while (incorrectKey);

                Inspection inspection = new Inspection(registrationNumber);


                if (inputKey.Key == ConsoleKey.J)
                {
                    inspection.Pass();

                    SaveInspectionToDatabase(inspection);

                    WriteLine("Inspektion godkänd");
                    Thread.Sleep(1000);
                }

                if (inputKey.Key == ConsoleKey.N)
                {
                    inspection.Fail();

                    SaveInspectionToDatabase(inspection);

                    WriteLine("Inspektion Ej godkänd");
                    Thread.Sleep(1000);
                }



            }
            else
            {
                Console.WriteLine("Reservation saknas");
                Thread.Sleep(1000);

            }



        }


        private static void CreateReservation()
        {
            bool incorrectKey = true;
            bool doNotExitLoop = true;

            do
            {

                SetCursorPosition(5, 7);
                WriteLine("Registreringsnummer: ");
                SetCursorPosition(5, 9);
                WriteLine("Datum (yyy-MM-dd hh:mm): ");
                SetCursorPosition("Registreringsnummer: ".Length + 4, 7);
                string registrationNumber = ReadLine();
                SetCursorPosition("Datum (yyy-MM-dd hh:mm): ".Length + 4, 9);
                DateTime dateTimeReservation = DateTime.Parse(ReadLine());

                Console.WriteLine("Är detta korrekt? (J)a eller (N)ej");

                ConsoleKeyInfo inputKey;
                do
                {

                    inputKey = ReadKey(true);

                    incorrectKey = !(inputKey.Key == ConsoleKey.J || inputKey.Key == ConsoleKey.N);



                } while (incorrectKey);


                if (inputKey.Key == ConsoleKey.J)
                {
                    Reservation reservation = new Reservation(registrationNumber, dateTimeReservation);

                    SaveReservationToDatabase(reservation);
                    doNotExitLoop = false;
                    Clear();
                    WriteLine("Reservation utförd");
                    Thread.Sleep(1000);
                }

                Clear();


            } while (doNotExitLoop);


        }


        private static void SaveInspectionToDatabase(Inspection inspection)
        {
            string query = @"
                          INSERT INTO [dbo].[Inspection]
                            ([RegistrationNumber],
                                    [PerformedAt],
                                    [Passed])
                           VALUES
                             (@RegistrationNumber,
                                 @PerformedAt,
                               @Passed         ) ";

            int passed =0;

            if (inspection.Passed)
            {
                passed = 1;
            }


            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RegistrationNUmber", inspection.RegistrationNumber);
                command.Parameters.AddWithValue("@PerformedAt", inspection.PerformedAt);
                command.Parameters.AddWithValue("@Passed", passed);


                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();


            }

        }

        private static void SaveReservationToDatabase(Reservation reservation)
        {
            string query = @"
                          INSERT INTO [dbo].[Reservation]
                            ([RegistrationNumber],
                                    [Date])
                           VALUES
                             (@RegistrationNumber,
                                 @Date) ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RegistrationNUmber", reservation.RegistrationNumber);
                command.Parameters.AddWithValue("@Date", reservation.Date);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();


            }


        }

        private static void ListInspections()
        {
            Inspections.AllInspections = GetInspectionsFromDatabase();


            WriteLine("Fordon        Utfört datum        Resultat");
            WriteLine(" ".PadLeft(50, '-'));

            string resultat;

            foreach (var inspection in Inspections.AllInspections)
            {
                if (inspection.Passed)
                {
                    resultat = "Godkänd";
                }
                else
                {
                    resultat = "Ej godkänd";
                }

                WriteLine($"{inspection.RegistrationNumber}        {inspection.PerformedAt}         {resultat}");

            }

            ReadKey();


        }

        private static List<Inspection> GetInspectionsFromDatabase()
        {


            List<Inspection> inspections = new List<Inspection>();

            string query = @"
                          SELECT 
                                     Id,               
                                    RegistrationNumber,
                                    PerformedAt,
                                    Passed
                           FROM Inspection";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {

                connection.Open();

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())

                {

                    int id = Convert.ToInt32(dataReader["Id"].ToString());
                    string registrationNumber = dataReader["RegistrationNumber"].ToString();
                    DateTime date = DateTime.Parse(dataReader["PerformedAt"].ToString());
                    bool passed = Convert.ToBoolean(dataReader["Passed"]);

                    inspections.Add(new Inspection(id, registrationNumber, date, passed));


                }

                connection.Close();


            }

            return inspections;



        }

        private static void ListReservations()
        {

            Reservations.AllReservations = GetReservationsFromDatabase();

            WriteLine("Id        Fordon        Datum");
            WriteLine(" ".PadLeft(50, '-'));

            foreach (var reservation in Reservations.AllReservations)
            {

                WriteLine($"{reservation.Id}        {reservation.RegistrationNumber}         {reservation.Date}");

            }

            ReadKey();
        }

        private static List<Reservation> GetReservationsFromDatabase()
        {


            List<Reservation> reservations = new List<Reservation>();

            string query = @"
                          SELECT 
                                     Id,               
                                    RegistrationNumber,
                                    Date

                           FROM Reservation";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {

                connection.Open();

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())

                {

                    int id = Convert.ToInt32(dataReader["Id"].ToString());
                    string registrationNumber = dataReader["RegistrationNumber"].ToString();
                    DateTime date = DateTime.Parse(dataReader["Date"].ToString());

                    reservations.Add(new Reservation(id, registrationNumber, date));


                }

                connection.Close();


            }

            return reservations;



        }


        private static void CreateTables()
        {
            string query = @"
                          DROP TABLE Reservation
                          DROP TABLE Inspection

                          CREATE TABLE Reservation(
                            [Id] [int] IDENTITY(1,1) NOT NULL,
                            [RegistrationNumber] [char](7) NOT NULL,
                            [Date] [datetime] NOT NULL)

                            CREATE TABLE Inspection(
                            [Id] [int] IDENTITY(1,1) NOT NULL,
                            [RegistrationNumber] [char](7) NOT NULL,
                            [PerformedAt] [datetime] NOT NULL,
                            [Passed] [bit] NOT NULL)
                          ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
               

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();


            }




        }


    }
}
