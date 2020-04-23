using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class Association
    {
        [Key]

        public int AssociationId {get;set;}
        [Required]
        public int WeddingId  {get;set;}
        [Required]
        public int UserId {get;set;}

        // Navigation properties
        public Wedding Wedding {get;set;}
        public User User {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;


    }
}
