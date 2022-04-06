using System;
using System.Linq;

namespace Intex313.Models
{
    public class EFAccidentRepository : IAccidentRepository
    {
        private AccidentDbContext _context;
        public EFAccidentRepository (AccidentDbContext ctx)
        {
            _context = ctx;
        }
        public IQueryable<Accident> Accidents => _context.Accidents;
        public void CreateAccident(Accident a)
        {
            _context.Add(a);
            _context.SaveChanges();
        }
        public void DeleteAccident(Accident a)
        {
            _context.Remove(a);
            _context.SaveChanges();
        }
        public void SaveAccident(Accident a)
        {
            _context.SaveChanges();
        }
    }
}
