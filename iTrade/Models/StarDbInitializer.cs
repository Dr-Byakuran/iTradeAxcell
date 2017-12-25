using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using iTrade.Models;

namespace iTrade.Models
{
    public class StarDbInitializer : DropCreateDatabaseIfModelChanges<StarDbContext>
    {
        protected override void Seed(StarDbContext context)
        {
            GetProductTypes().ForEach(p => context.ProductGroups.Add(p));
            GetWarehouses().ForEach(wa => context.Warehouses.Add(wa));

            context.GstRate.Add(GetGstRate());


            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Clients', RESEED, 1001)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Suppliers', RESEED, 301)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Staffs', RESEED, 6001)");

            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Products', RESEED, 10001)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('Pricebreaks', RESEED, 101)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('ProductBundles', RESEED, 301)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('ProductFOCs', RESEED, 501)");


            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('INVs', RESEED, 2001)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('INVDETs', RESEED, 1001)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('SalesOrders', RESEED, 10001)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('KIVs', RESEED, 2001)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('KIVDETs', RESEED, 201)");

            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('KivOrders', RESEED, 5001)");

            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('ProductGroups', RESEED, 1)");

            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('StockTakes', RESEED, 1001)");


        }

        private static List<ProductGroup> GetProductTypes()
        {
            var pt = new List<ProductGroup> {
                new ProductGroup
                {
                    ID = 1,
                    ProductType = "Ceiling Fans",
                },
                new ProductGroup
                {
                    ID = 2,
                    ProductType = "Water Heaters",
                },
                new ProductGroup
                {
                    ID = 3,
                    ProductType = "LED Lights",
                },
                new ProductGroup
                {
                    ID = 4,
                    ProductType = "Others",
                }


            };

            return pt;
        }

        private static GST GetGstRate() 
        {
            var gst = new GST();
            gst.ID = 1;
            gst.GstRate = 7;

            return gst;
        }

        private static List<Warehouse> GetWarehouses()
        {
            var p = new List<Warehouse> {
                new Warehouse
                {
                    LocationID = 11,
                    LocationName = "Main Warehouse",
                    LocationType = "Default",
                    Addr1 = "",
                    Addr2 = "",
                    Addr3 = "",
                    PostalCode = "",
                    Country = "Singapore",
                    PhoneNo = "",
                    FaxNo = "",
                    ContactPerson = "",
                    IsActive = true

                },
                new Warehouse
                {
                    LocationID = 12,
                    LocationName = "Showroom",
                    LocationType = "Default",
                    Addr1 = "",
                    Addr2 = "",
                    Addr3 = "",
                    PostalCode = "",
                    Country = "Singapore",
                    PhoneNo = "",
                    FaxNo = "",
                    ContactPerson = "",
                    IsActive = true
                },

            };

            return p;
        }




    }
}