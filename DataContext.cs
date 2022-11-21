using System;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Model;

namespace MinimalApi
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Todo> Todos => Set<Todo>();
    }
}

