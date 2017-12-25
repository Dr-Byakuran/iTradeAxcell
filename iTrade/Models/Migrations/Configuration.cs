using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Migrations;

namespace iTrade.Models.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<iTrade.Models.StarDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "iTrade.Models.StarDbContext";
            MigrationsNamespace = "iTrade.Models";
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}