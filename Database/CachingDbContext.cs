using CachingWebApi.Models;
using Microsoft.EntityFrameworkCore;
namespace CachingWebApi.Database
{
	public class CachingDbContext: DbContext
	{
		public virtual DbSet<Driver> Drivers{get;set;}
		public CachingDbContext(DbContextOptions<CachingDbContext> options) : base(options)
		{}
	}
}
