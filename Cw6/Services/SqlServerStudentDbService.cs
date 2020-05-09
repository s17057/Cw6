using Cw6.DTO.Requests;
using Cw6.DTO.Responses;
using Cw6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Cw6.Services
{
    public class SqlServerStudentDbService : IStudentDbService
    {
        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            using (var connect = new SqlConnection("Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s17057;Integrated Security=True"))
            using (var commend = new SqlCommand())
            {
                var response = new EnrollStudentResponse();
                commend.Connection = connect;
                connect.Open();
                var tran = connect.BeginTransaction();
                commend.Transaction = tran;
                commend.CommandText = "SELECT IdStudy FROM studies WHERE name=@name";
                commend.Parameters.AddWithValue("name", request.Studies);
                var dr = commend.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    tran.Rollback();
                    throw new ArgumentException("Studia " + request.Studies + " nie isnieją");
                }
                int idstudies = (int)dr["IdStudy"];
                response.IdStudies = idstudies;
                response.Semester = 1;
                dr.Close();
                commend.Parameters.Clear();
                commend.CommandText = "SELECT TOP 1 IdEnrollment, StartDate FROM enrollment WHERE semester = 1 AND IdStudy = @idStudy order by StartDate desc";
                commend.Parameters.AddWithValue("idStudy", idstudies);
                dr = commend.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    commend.CommandText = "INSERT INTO ENROLLMENT(IdEnrollment,Semester,IdStudy,StartDate) OUTPUT INSERTED.IdEnrollment VALUES((SELECT MAX(E.IdEnrollment) FROM Enrollment E) + 1,1,@idStudy,@startDate";
                    var studiesStartDate = DateTime.Now;
                    commend.Parameters.AddWithValue("startDate", studiesStartDate);
                    dr = commend.ExecuteReader();
                    dr.Read();
                    response.IdEnrollment = (int)dr["IdEnrollment"];
                    response.StartDate = studiesStartDate;
                }
                else
                {
                    response.IdEnrollment = (int)dr["IdEnrollment"];
                    response.StartDate = (DateTime)dr["StartDate"];
                }
                dr.Close();
                commend.Parameters.Clear();
                commend.CommandText = "INSERT INTO StudentAPBD(IndexNumber,FirstName,LastName,BirthDate,IdEnrollment) VALUES(@index,@fname,@lname,@bdate,@idenrollment)";
                commend.Parameters.AddWithValue("index", request.IndexNumber);
                commend.Parameters.AddWithValue("fname", request.FirstName);
                commend.Parameters.AddWithValue("lname", request.LastName);
                commend.Parameters.AddWithValue("bdate", request.BirthDate);
                commend.Parameters.AddWithValue("idenrollment", response.IdEnrollment);
                try
                {
                    dr = commend.ExecuteReader();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    dr.Close();
                    tran.Rollback();
                    throw new ArgumentException("Duplikat numeru indeksu");
                }
                response.IndexNumber = request.IndexNumber;
                dr.Close();
                tran.Commit();
                return response;
            }
        }

        public PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request)
        {
            using (var connect = new SqlConnection("Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s17057;Integrated Security=True"))
            using (var commend = new SqlCommand())
            {
                commend.Connection = connect;
                connect.Open();
                commend.CommandText = "EXEC PromoteStudents @Studies, @Semester";
                commend.Parameters.AddWithValue("Studies", request.Studies);
                commend.Parameters.AddWithValue("Semester", request.Semester);
                var dr = commend.ExecuteReader();

                if (dr.Read())
                {
                    return new PromoteStudentsResponse
                    {
                        IdEnrollment = (int)dr["IdEnrollment"],
                        Semester = (int)dr["Semester"],
                        IdStudy = (int)dr["IdStudy"],
                        StartDate = (DateTime)dr["StartDate"]
                    };
                }
                else
                {
                    return null;
                }
            }
        }
        public Student GetStudent(String id)
        {

            using (var connect = new SqlConnection("Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s17057;Integrated Security=True"))
            using (var commend = new SqlCommand())
            {
                var st = new Student();
                commend.Connection = connect;
                connect.Open();
                commend.CommandText = "SELECT * FROM StudentAPBD s LEFT JOIN ENROLLMENT e ON s.IdEnrollment = e.IdEnrollment LEFT JOIN STUDIES st on e.IdStudy = st.IdStudy WHERE IndexNumber LIKE @id";
                commend.Parameters.AddWithValue("id", id);
                var dr = commend.ExecuteReader();
                if (dr.Read())
                {
                    return new Student
                    {
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        IndexNumber = dr["IndexNumber"].ToString(),
                        BirthDate = Convert.ToDateTime(dr["BirthDate"].ToString()),
                        Studies = dr["Name"].ToString(),
                        Semester = Convert.ToInt32(dr["Semester"].ToString())
                    };
                }
                else
                {
                    return null;
                }
            }
        }
        public IEnumerable<Student> GetStudents()
        {

            using (var connect = new SqlConnection("Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s17057;Integrated Security=True"))
            using (var commend = new SqlCommand())
            {
                var list = new List<Student>();
                commend.Connection = connect;
                connect.Open();
                commend.CommandText = "SELECT * FROM StudentAPBD s LEFT JOIN ENROLLMENT e ON s.IdEnrollment = e.IdEnrollment LEFT JOIN STUDIES st on e.IdStudy = st.IdStudy";
                var dr = commend.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        list.Add(new Student
                        {
                            FirstName = dr["FirstName"].ToString(),
                            LastName = dr["LastName"].ToString(),
                            IndexNumber = dr["IndexNumber"].ToString(),
                            BirthDate = Convert.ToDateTime(dr["BirthDate"].ToString()),
                            Studies = dr["Name"].ToString(),
                            Semester = Convert.ToInt32(dr["Semester"].ToString())
                        });
                    }
                    return list;
                }
                else
                {
                    return null;
                }

            }
        }
        public Boolean CheckStudent(String indexNumber)
        {
            using (var connect = new SqlConnection("Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s17057;Integrated Security=True"))
            using (var commend = new SqlCommand())
            {
                commend.Connection = connect;
                connect.Open();
                commend.CommandText = "SELECT * FROM StudentAPBD WHERE IndexNumber LIKE @indexNumber";
                commend.Parameters.AddWithValue("indexNumber", indexNumber);
                var dr = commend.ExecuteReader();
                return dr.HasRows;
            }
        }
    }
}
