using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace iTrade.Models
{
    public class StarDbContext : DbContext
    {
        public StarDbContext()
            : base("StarConnection")
        {
            Database.SetInitializer<iTrade.Models.StarDbContext>(new MigrateDatabaseToLatestVersion<StarDbContext, iTrade.Models.Migrations.Configuration>());
            Database.Initialize(false);
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<StudentGuardian> StudentGuardians { get; set; }
        //    public DbSet<Membership> Memberships { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CoursePrice> CoursePrices { get; set; }
        public DbSet<CourseBundle> CourseBundles { get; set; }

        public DbSet<Pricebook> Pricebooks { get; set; }
        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<Attachment> Attachment { get; set; }
        public DbSet<FileManager> FileManagers { get; set; }

        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<ClassAttendance> ClassAttendances { get; set; }
        public DbSet<ClassAttendee> ClassAttendees { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<BillItem> BillItems { get; set; }

        public DbSet<TimeSheet> TimeSheets { get; set; }
        public DbSet<TimeClock> TimeClocks { get; set; }

        public DbSet<CourseOrder> CourseOrders { get; set; }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientContact> ClientContacts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
 
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PoINV> PoINVs { get; set; }
        public DbSet<PoINVDET> PoINVDETs { get; set; }
        public DbSet<PoKIVDET> PoKIVDETs { get; set; }

        public DbSet<PoKIV> PoKIVs { get; set; }
        public DbSet<PoKivOrder> PoKivOrders { get; set; }
        public DbSet<PoKivOrderDet> PoKivOrderDets { get; set; }
 
        public DbSet<Product> Products { get; set; }

        public DbSet<PriceOption> PriceOptions { get; set; }
        public DbSet<Pricebreak> Pricebreaks { get; set; }
        public DbSet<Productbundle> Productbundles { get; set; }
        public DbSet<ProductFOC> ProductFOCs { get; set; }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockDet> StockDets { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseStock> WarehouseStocks { get; set; }

        public DbSet<StockTake> StockTakes { get; set; }
        public DbSet<StockTakeDet> StockTakeDets { get; set; }

        public DbSet<ProductGroup> ProductGroups { get; set; }

        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }

        public DbSet<INV> INVs { get; set; }
        public DbSet<INVDET> INVDETs { get; set; }
        public DbSet<KIVDET> KIVDETs { get; set; }

        public DbSet<CreditNote> CreditNotes { get; set; }
        public DbSet<CreditNoteDet> CreditNoteDets { get; set; }

        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }
        public DbSet<SalesPaymentMethod> SalesPaymentMethods { get; set; }

        public DbSet<PaymentVoucher> PaymentVouchers { get; set; }
        public DbSet<PurchasePaymentMethod> PurchasePaymentMethods { get; set; }

        public DbSet<ExchangeOrder> ExchangeOrders { get; set; }
        public DbSet<ExchangeOrderDet> ExchangeOrderDets { get; set; }

        public DbSet<KIV> KIVs { get; set; }
        public DbSet<KivOrder> KivOrders { get; set; }
        public DbSet<KivExchangeOrder> KivExchangeOrders { get; set; }
        public DbSet<KivOrderDet> KivOrderDets { get; set; }
        
        //public DbSet<KIVDelivery> KIVDelivery { get; set; }
        //public DbSet<KIVDeliveryDetail> KIVDeliveryDetails { get; set; }

        public DbSet<Staff> Staffs { get; set; }
        public DbSet<GST> GstRate { get; set; }
        public DbSet<CurrencySetting> CurrencySettings { get; set; }

        public DbSet<ClientCreditSetting> ClientCreditSetting { get; set; }
        public DbSet<VendorCreditSetting> VendorCreditSetting { get; set; }

        public DbSet<PrintHistory> PrintHistory { get; set; }
        public System.Data.Entity.DbSet<iTrade.Models.ReturnedItem> ReturnedItems { get; set; }
        public System.Data.Entity.DbSet<iTrade.Models.ReturnedItemDetail> ReturnedItemDetails { get; set; }

        public System.Data.Entity.DbSet<iTrade.Models.ReceivedItem> ReceivedItems { get; set; }
        public System.Data.Entity.DbSet<iTrade.Models.ReceivedItemDetail> ReceivedItemDetails { get; set; }

        public System.Data.Entity.DbSet<iTrade.Models.UserCompany> UserCompanies { get; set; }

        public System.Data.Entity.DbSet<iTrade.Models.Company> Companies { get; set; }

        public System.Data.Entity.DbSet<iTrade.Models.CustomSetting> CustomSettings { get; set; }
        public System.Data.Entity.DbSet<iTrade.Models.LoanManagement> LoanManagements { get; set; }
        public System.Data.Entity.DbSet<iTrade.Models.LoanManagementDetail> LoanManagementDetails { get; set; }

        public System.Data.Entity.DbSet<iTrade.Models.DrOrder> DrOrders { get; set; }
        public System.Data.Entity.DbSet<iTrade.Models.DrOrderDet> DrOrderDets { get; set; }
        public System.Data.Entity.DbSet<iTrade.Models.SerialNumber> SerialNumbers { get; set; }
    }
}