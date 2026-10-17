using Microsoft.EntityFrameworkCore;
using ModelLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Context
{
    public class FundooDbContext : DbContext
    {
        public FundooDbContext(DbContextOptions<FundooDbContext> options) : base(options) { }
        public DbSet<UserEntity> Users => Set<UserEntity>();
        public DbSet<NoteEntity> Notes => Set<NoteEntity>();
    }
}
