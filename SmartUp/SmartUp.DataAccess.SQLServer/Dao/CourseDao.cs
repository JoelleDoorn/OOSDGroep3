﻿using SmartUp.DataAccess.SQLServer.Util;
using System.Data.SqlClient;

namespace SmartUp.DataAccess.SQLServer.Dao
{
    public class CourseDao
    {
        private static CourseDao? instance;
        private CourseDao()
        {
        }

        public static CourseDao GetInstance()
        {
            if (instance == null)
            {
                instance = new CourseDao();
            }
            return instance;
        }

        public void FillTable()
        {
            List<string> itCourses = new List<string>
            {
                "Introduction to Computer Science",
                "Data Structures and Algorithms",
                "Web Development Fundamentals",
                "Database Management Systems",
                "Network Fundamentals",
                "Cybersecurity Basics",
                "Software Engineering Principles",
                "Mobile App Development",
                "Cloud Computing",
                "Artificial Intelligence and Machine Learning",
                "DevOps Practices",
                "IT Project Management",
                "Computer Graphics and Visualization",
                "Human-Computer Interaction",
                "Game Development",
                "Blockchain Technology",
                "Internet of Things (IoT)",
                "Big Data Analytics",
                "Digital Marketing for IT Professionals",
                "IT Ethics and Cybersecurity Policies",
                "Operating Systems Concepts",
                "Computer Architecture",
                "Computer Networks",
                "Software Testing and Quality Assurance",
                "Object-Oriented Programming",
                "Web Design and User Experience",
                "Linux System Administration",
                "Windows Server Administration",
                "Data Warehousing and Data Mining",
                "Computer Forensics",
                "Network Security",
                "Advanced Web Technologies",
                "Embedded Systems Programming",
                "Computer Vision",
                "Quantum Computing",
                "Mobile Game Development",
                "Cloud Security",
                "Natural Language Processing",
                "IT Governance and Compliance",
                "IT Service Management",
                "Parallel and Distributed Computing",
                "Wireless Communication",
                "IT Strategy and Planning",
                "Biometric Systems",
                "Virtual Reality and Augmented Reality",
                "IT Disaster Recovery Planning",
                "Health Informatics",
                "IT for Sustainable Development"
            };
            using SqlConnection con = DatabaseConnection.GetConnection();
            try
            {
                con.Open();
                Random random = new Random();
                foreach (string courseName in itCourses)
                {
                    int credits = random.Next(1, 6);
                    string query = "INSERT INTO course ([name], credits) VALUES (@name, @credits);";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@name", courseName);
                        command.Parameters.AddWithValue("@credits", credits);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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
