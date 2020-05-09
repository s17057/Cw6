using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cw6.DTO.Requests;
using Cw6.DTO.Responses;
using Cw6.Models;

namespace Cw6.Services
{
    public interface IStudentDbService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request);
        Student GetStudent(String id);
        IEnumerable<Student> GetStudents();
        Boolean CheckStudent(String indexNumber);
    }
}
