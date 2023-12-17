using Microsoft.Data.SqlClient;
using SmartUp.DataAccess.SQLServer.Model;
using SmartUp.DataAccess.SQLServer.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUp.DataAccess.SQLServer.Dao
{
    public class SemesterCriteriaPercentageDao
    {
        private static SemesterCriteriaPercentageDao? instance;
        private SemesterCriteriaPercentageDao()
        {
        }

        public static SemesterCriteriaPercentageDao GetInstance()
        {
            if (instance == null)
            {
                instance = new SemesterCriteriaPercentageDao();
            }
            return instance;
        }

        public void FillTable()
        {
            List<string> semesterPart1 = new List<string>
            {
                "Introduction to Programming",
                "Data Structures and Algorithms",
                "Object-Oriented Programming",
                "Database Management Systems",
                "Web Development and Design",
                "Software Engineering Principles",
                "Networking and Security"
            };
            List<string> semesterPart2 = new List<string>
            {
               "Artificial Intelligence and Machine Learning",
                "Cloud Computing Technologies",
                "Capstone Project and Final Assessment"
            };
            using SqlConnection con = DatabaseConnection.GetConnection();

            try
            {
                con.Open();
                int counter = 0;
                int percentagePart1 = 50;
                foreach (string semester in semesterPart1)
                {

                    string query = "INSERT INTO semesterCriteriaPercentage(SemesterName, RequiredSemesterName, RequiredPrecentage) " +
                    "VALUES(@semesterName, @requiredSemester1, @percentage); " +
                    "INSERT INTO semesterCriteriaPercentage(SemesterName, RequiredSemesterName, RequiredPrecentage) " +
                    "VALUES(@semesterName, @requiredSemester2, @percentage); ";


                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@semesterName", semester);
                        command.Parameters.AddWithValue("@requiredSemester1", "Basic programming 1");
                        command.Parameters.AddWithValue("@requiredSemester2", "Basic programming 2");
                        command.Parameters.AddWithValue("@percentage", percentagePart1);

                        command.ExecuteNonQuery();
                    }
                    counter++;
                    if(counter == 4)
                    {
                        counter = 0;
                        percentagePart1 = 100;
                    }
                }
                int counter2 = 0;
                int percentagePart2 = 50;
                foreach (string semester in semesterPart2)
                {

                    string query = "INSERT INTO semesterCriteriaPercentage (SemesterName, RequiredSemesterName, RequiredPrecentage) " +
                        "VALUES (@semesterName, @requiredSemester, @percentage); ";


                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@semesterName", semester);
                        command.Parameters.AddWithValue("@requiredSemester", "Data Structures and Algorithms");
                        command.Parameters.AddWithValue("@percentage", percentagePart2);

                        command.ExecuteNonQuery();
                    }
                    if (counter == 2)
                    {
                        percentagePart2 = 100;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
            }
            finally
            {
                if (con.State != System.Data.ConnectionState.Closed)
                {
                    con.Close();
                }
            }
        }
    }
}
