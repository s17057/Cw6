using Cw6.Models;
using System.Collections.Generic;

namespace Cw6.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}
