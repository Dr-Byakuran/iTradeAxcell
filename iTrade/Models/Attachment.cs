using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTrade.Models
{
    public class Attachment
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AttachmentId { get; set; }

        [StringLength(200)]
        public string AttachmentType { get; set; }
        public string Path { get; set; }
    }
}