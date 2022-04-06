using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex313.Models
{
    public class AccidentDbContext : IdentityDbContext<IdentityUser>
    {
        public AccidentDbContext(DbContextOptions<AccidentDbContext> options) : base(options)
        {

        }

        public DbSet<Accident> Accidents { get; set; }
    }
}
