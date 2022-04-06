using System;
using System.Linq;

namespace Intex313.Models
{
    public interface IAccidentRepository
    {
        IQueryable<Accident> Accidents { get; }
        void SaveAccident(Accident a);
        void CreateAccident(Accident a);
        void DeleteAccident(Accident a);
    }
}
