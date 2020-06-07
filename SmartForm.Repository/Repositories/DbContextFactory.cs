using SmartForm.Domain;
using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Repository.Repositories
{
    public class DbContextFactory : IDbContextFactory
    {
        private string contextName;
        private string connectionString;
        public DbContextFactory(string contextName, string connectionString)
        {
            this.contextName = contextName;
            this.connectionString = connectionString;
        }

        public DbContext GetContext()
        {
            switch (this.contextName)
            {
                case "SmartFormEntities":
                case "DefaultContext":
                default:
                    return new SmartFormEntities(this.connectionString);
            }
        }
    }
}
