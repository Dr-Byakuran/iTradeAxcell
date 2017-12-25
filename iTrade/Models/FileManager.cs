using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class FileManager
    {
        public FileManager()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string No { get; set; }
        public string LoginName { get; set; }
        public string Path { get; set; }
        public string BusinessType { get; set; }
    }
}