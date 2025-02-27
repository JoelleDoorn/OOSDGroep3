﻿using Microsoft.Data.SqlClient;
using SmartUp.Core.Constants;
using SmartUp.DataAccess.SQLServer.Model;
using SmartUp.DataAccess.SQLServer.Util;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SmartUp.DataAccess.SQLServer.Dao
{
    public class GradeDao
    {
        private static GradeDao? instance;
        private GradeDao()
        {
        }

        public static GradeDao GetInstance()
        {
            if (instance == null)
            {
                instance = new GradeDao();
            }
            return instance;
        }

        public void FillTable()
        {
            using SqlConnection con = DatabaseConnection.GetConnection();
            try
            {
                con.Open();
                Random random = new Random();

                List<String> studentIds = StudentDao.GetInstance().GetAllStudentIds();
                List<String> courseNames = CourseDao.GetInstance().GetAllCourseNames();
                foreach (string studentId in studentIds)
                {
                    List<String> selectedCourses = courseNames.Distinct().Take(random.Next(5) + 1).ToList();
                    foreach (string courseName in selectedCourses)
                    {
                        int randomInt = random.Next((int)(1.0m / 0.1m), (int)(10.0m / 0.1m));
                        decimal randomGrade = randomInt * 0.1m; ;
                        DateTime startDate = new DateTime(2000, 1, 1);
                        DateTime endDate = new DateTime(2022, 12, 31);
                        int range = (endDate - startDate).Days;
                        int randomDays = random.Next(range);
                        TimeSpan randomTimeSpan = new TimeSpan(randomDays, random.Next(24), random.Next(60), random.Next(60));
                        DateTime randomDateTime = startDate + randomTimeSpan;
                        string query = "INSERT INTO grade (studentId, courseName, attempt, grade, isDefinitive, date) " +
                            "VALUES (@StudentId, @CourseName, @Attempt, @Grade, @IsDefinitive, @Date)";
                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@StudentId", studentId);
                            command.Parameters.AddWithValue("@CourseName", courseName);
                            command.Parameters.AddWithValue("@Attempt", 1);
                            command.Parameters.AddWithValue("@Grade", randomGrade);
                            command.Parameters.AddWithValue("@IsDefinitive", 0);
                            command.Parameters.AddWithValue("@Date", randomDateTime);
                            command.ExecuteNonQuery();
                        }
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
                    DatabaseConnection.CloseConnection(con);
                }
            }
        }

        public List<Grade> GetGradesByStudentId(string studentId)
        {
            List<Grade> grades = new List<Grade>();
            string query = "SELECT grade.grade, grade.isDefinitive, grade.date, grade.courseName, course.credits, grade.attempt " +
               "FROM grade JOIN course ON course.name = grade.courseName " +
               "WHERE grade.studentId = @StudentId";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", studentId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                decimal grade = Convert.ToDecimal(reader["grade"]);
                                bool isDefinitive = Convert.ToBoolean(reader["isDefinitive"]);
                                DateTime date = Convert.ToDateTime(reader["date"]);
                                string courseName = reader["courseName"].ToString();
                                int credits = Convert.ToInt32(reader["credits"]);
                                int attempt = Int32.Parse(reader["attempt"].ToString());

                                grades.Add(new Grade(grade, isDefinitive, date, courseName, credits, attempt));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }
            }
            return grades;
        }
        public ObservableCollection<GradeTeacher> GetGradesByCourse(string CourseName)
        {
            ObservableCollection<GradeTeacher> grades = new ObservableCollection<GradeTeacher>();
            string query = "SELECT student.id, student.firstname, student.lastname, student.infix, grade.grade, grade.isDefinitive, grade.courseName " +
                "FROM student " +
                "JOIN grade ON student.id = grade.studentId AND grade.courseName = @CourseName " +
                "JOIN course ON course.name = @CourseName " +
                "UNION SELECT student.id, student.firstname, student.lastname, student.infix,  grade.grade,  Grade.isDefinitive, semesterCourse.courseName " +
                "FROM student " +
                "JOIN registrationSemester ON student.id = registrationSemester.studentId " +
                "JOIN semesterCourse ON registrationSemester.semesterName = semesterCourse.semesterName " +
                "LEFT JOIN grade ON student.id = grade.studentId AND semesterCourse.courseName = grade.courseName " +
                "WHERE semesterCourse.courseName = @CourseName AND grade.studentId IS NULL;";
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                try
                {

                    if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CourseName", CourseName);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string studentId = reader["id"].ToString();
                                string firstName = reader["firstname"].ToString();
                                string lastName = reader["lastname"].ToString();
                                string infix = reader["infix"].ToString();
                                string courseName = reader["courseName"].ToString();
                                decimal? grade = null;
                                string? isDefinitive = null;
                                bool hadGrade = false;
                                if (reader["grade"] != DBNull.Value && reader["isDefinitive"] != DBNull.Value)
                                {
                                    grade = Convert.ToDecimal(reader["grade"]);
                                    if (Convert.ToBoolean(reader["isDefinitive"]) == false)
                                    {
                                        isDefinitive = "Voorlopig";
                                    }
                                    else
                                    {
                                        isDefinitive = "Definitief";
                                    }
                                    hadGrade = true;
                                }

                                if (!hadGrade)
                                {
                                    grades.Add(new GradeTeacher(courseName, new Student(firstName, lastName, infix, studentId)));
                                }
                                else
                                {
                                    grades.Add(new GradeTeacher(grade.GetValueOrDefault(), isDefinitive, courseName, new Student(firstName, lastName, infix, studentId)));
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }
            }

            return grades;
        }
        public ObservableCollection<GradeTeacher> GetGradesByCourseAndClass(string CourseName, string ClassName)
        {
            ObservableCollection<GradeTeacher> grades = new ObservableCollection<GradeTeacher>();
            string query = "SELECT student.id, student.firstname, student.lastname, student.infix, grade.grade, grade.isDefinitive, grade.courseName " +
                "FROM student " +
                "JOIN grade ON student.id = grade.studentId AND grade.courseName = @CourseName " +
                "JOIN course ON course.name = @CourseName " +
                "WHERE student.class = @ClassName " +
                "UNION SELECT student.id AS studentId, student.firstname, student.lastname, student.infix, grade.grade,  grade.isDefinitive, semesterCourse.courseName " +
                "FROM student " +
                "JOIN registrationSemester ON student.id = registrationSemester.studentId " +
                "JOIN semesterCourse ON registrationSemester.semesterName = semesterCourse.semesterName " +
                "LEFT JOIN grade ON student.id = grade.studentId AND semesterCourse.courseName = grade.courseName " +
                "WHERE semesterCourse.courseName = @CourseName AND student.class = @ClassName AND grade.studentId IS NULL;";
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CourseName", CourseName);
                        command.Parameters.AddWithValue("@ClassName", ClassName);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string studentId = reader["id"].ToString();
                                string firstName = reader["firstname"].ToString();
                                string lastName = reader["lastname"].ToString();
                                string infix = reader["infix"].ToString();
                                string courseName = reader["courseName"].ToString();
                                decimal? grade = null;
                                string? isDefinitive = null;
                                bool hadGrade = false;
                                if (reader["grade"] != DBNull.Value && reader["isDefinitive"] != DBNull.Value)
                                {
                                    grade = Convert.ToDecimal(reader["grade"]);
                                    if (Convert.ToBoolean(reader["isDefinitive"]) == false)
                                    {
                                        isDefinitive = "Voorlopig";
                                    }
                                    else
                                    {
                                        isDefinitive = "Definitief";
                                    }
                                    hadGrade = true;
                                }

                                if (!hadGrade)
                                {
                                    grades.Add(new GradeTeacher(courseName, new Student(firstName, lastName, infix, studentId)));
                                }
                                else
                                {
                                    grades.Add(new GradeTeacher(grade.GetValueOrDefault(), isDefinitive, courseName, new Student(firstName, lastName, infix, studentId)));
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }
            }

            return grades;
        }

        public Grade GetGradeByAttemptByCourseNameByStudentId(string studentId, string courseName, int attempt)
        {
            string query = "SELECT grade.grade, grade.isDefinitive, grade.date, grade.courseName, course.credits, grade.attempt " +
                           "FROM grade JOIN course ON course.name = grade.courseName " +
                           "WHERE grade.studentId = @StudentId AND grade.courseName = @CourseName AND grade.attempt = @Attempt";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", studentId);
                        command.Parameters.AddWithValue("@CourseName", courseName);
                        command.Parameters.AddWithValue("@Attempt", attempt);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                decimal grade = Convert.ToDecimal(reader["grade"]);
                                bool isDefinitive = Convert.ToBoolean(reader["isDefinitive"]);
                                DateTime date = Convert.ToDateTime(reader["date"]);
                                string courseNameResult = reader["courseName"].ToString();
                                int credits = Convert.ToInt32(reader["credits"]);
                                int attemptResult = Int32.Parse(reader["attempt"].ToString());
                                return new Grade(grade, isDefinitive, date, courseNameResult, credits, attemptResult);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }
            }
            return null;
        }

        public Dictionary<string, decimal> ReturnGradesAsDictionaryByStudentId(string studentId)
        {
            Dictionary<string, decimal> grades = new Dictionary<string, decimal>();
            string query = "SELECT courseName, grade FROM grade WHERE studentId = @StudentId";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", studentId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string courseName = reader["courseName"].ToString();
                                decimal grade = Convert.ToDecimal(reader["grade"]);

                                grades.Add(courseName, grade);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }
            }

            return grades;
        }

        public ObservableCollection<GradeTeacher> GetGradesByClass(string ClassName)
        {
            ObservableCollection<GradeTeacher> grades = new ObservableCollection<GradeTeacher>();
            string query = "SELECT student.id, student.firstname, student.lastname, student.infix, grade.grade, grade.isDefinitive, grade.courseName " +
                "FROM student " +
                "JOIN grade ON student.id = grade.studentId " +
                "WHERE student.class = @ClassName " +
                "UNION SELECT student.id AS studentId, student.firstname, student.lastname, student.infix, grade.grade,  grade.isDefinitive, semesterCourse.courseName " +
                "FROM  student " +
                "JOIN  registrationSemester ON student.id = registrationSemester.studentId " +
                "JOIN  semesterCourse ON registrationSemester.semesterName = semesterCourse.semesterName " +
                "LEFT JOIN  grade ON student.id = grade.studentId AND semesterCourse.courseName = grade.courseName " +
                "WHERE student.class = @ClassName AND grade.studentId IS NULL; ";
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClassName", ClassName);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string studentId = reader["id"].ToString();
                                string firstName = reader["firstname"].ToString();
                                string lastName = reader["lastname"].ToString();
                                string infix = reader["infix"].ToString();
                                string courseName = reader["courseName"].ToString();
                                decimal? grade = null;
                                string? isDefinitive = null;
                                bool hadGrade = false;
                                if (reader["grade"] != DBNull.Value && reader["isDefinitive"] != DBNull.Value)
                                {
                                    grade = Convert.ToDecimal(reader["grade"]);
                                    if (Convert.ToBoolean(reader["isDefinitive"]) == false)
                                    {
                                        isDefinitive = "Voorlopig";
                                    }
                                    else
                                    {
                                        isDefinitive = "Definitief";
                                    }
                                    hadGrade = true;
                                }

                                if (!hadGrade)
                                {
                                    grades.Add(new GradeTeacher(courseName, new Student(firstName, lastName, infix, studentId)));
                                }
                                else
                                {
                                    grades.Add(new GradeTeacher(grade.GetValueOrDefault(), isDefinitive, courseName, new Student(firstName, lastName, infix, studentId)));
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }
            }

            return grades;
        }
        public void UpdateGrade(string studentId, string course, decimal grade)
        {
            string query = "MERGE INTO grade AS target " +
                  "USING (SELECT @studentId AS StudentID, @Course AS CourseName) AS source " +
                  "ON target.studentId = source.StudentID AND target.courseName = source.CourseName " +
                  "WHEN MATCHED THEN " +
                  "    UPDATE SET target.grade = @grade, target.date = CURRENT_TIMESTAMP " +
                  "WHEN NOT MATCHED THEN " +
                  "    INSERT (studentId, courseName, attempt, grade, isDefinitive, date) " +
                  "    VALUES (@studentId, @Course, 1, @grade, 0, CURRENT_TIMESTAMP);";


            using (SqlConnection? connection = DatabaseConnection.GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@studentId", studentId);
                        command.Parameters.AddWithValue("@Course", course);
                        command.Parameters.AddWithValue("@grade", grade);


                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }

            }
        }
        public void UpdateIsDefinitiveByCourseAndClass(string course, string StudentClass)
        {
            string query = "UPDATE grade " +
                "SET isDefinitive = 1 " +
                "FROM grade " +
                "JOIN student ON grade.studentId = student.id " +
                "WHERE grade.courseName = @Course AND student.class = @Class;";

            using (SqlConnection? connection = DatabaseConnection.GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Course", course);
                        command.Parameters.AddWithValue("@Class", StudentClass);


                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }

            }
        }

        public void UpdateIsDefinitiveByCourse(string course)
        {
            string query = "UPDATE grade " +
                "SET isDefinitive = 0 " +
                "WHERE grade.courseName = @Course";

            using (SqlConnection? connection = DatabaseConnection.GetConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); };
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Course", course);

                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in method {System.Reflection.MethodBase.GetCurrentMethod().Name}: {ex.Message}");
                }
                finally
                {
                    DatabaseConnection.CloseConnection(connection);
                }

            }
        }

        public bool IsGradePassed(SqlConnection connection, string courseName)
        {
            string query = "SELECT grade FROM grade   WHERE studentId = @studentId AND courseName = @courseName";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@studentId", Constants.STUDENT_ID);
                command.Parameters.AddWithValue("@courseName", courseName);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal grade = Convert.ToDecimal(reader["grade"]);
                        if (grade >= 5.50m) return true;
                    }
                }
            }
            return false;
        }

        public bool HasObtainedGrade(SqlConnection connection, string semesterName)
        {
            string query = "SELECT grade.studentId, grade.courseName, grade.grade, grade.isDefinitive FROM grade JOIN semesterCourse ON grade.courseName = semesterCourse.courseName WHERE semesterCourse.semesterName = @semesterName AND grade.studentId = @studentId";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@semesterName", semesterName);
                command.Parameters.AddWithValue("@studentId", Constants.STUDENT_ID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}