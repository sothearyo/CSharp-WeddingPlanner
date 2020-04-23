using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]

        public int WeddingId {get;set;}
        [Required]
        public string WedderOne {get;set;}
        [Required]
        public string WedderTwo {get;set;}
        [Required]
        [Date(ErrorMessage="Wedding date must be in the future.")]
        public DateTime Date {get;set;}
        [Required]
        public string Address {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        // Foreign Key
        public int UserId {get;set;}

        // Navigation for one to many - one user can create many weddings
        public User UserWedding {get;set;}

        // Navigation Properties for many to many
        public List<Association> AllUsers {get;set;}

        // Not mapped properties
        [NotMapped]
        public string WeddingName
        {
            get
            {   
                int EndIndexOne = 0;
                if (this.WedderOne.Contains(" "))
                {
                    EndIndexOne = this.WedderOne.IndexOf(" ");
                }
                else
                {
                    EndIndexOne = this.WedderOne.Length;
                }

                int EndIndexTwo = 0;
                if (this.WedderTwo.Contains(" "))
                {
                    EndIndexTwo = this.WedderTwo.IndexOf(" ");
                }
                else
                {
                    EndIndexTwo = this.WedderTwo.Length;
                }

                string str1 = this.WedderOne.Substring(0,EndIndexOne);
                string str2 = this.WedderTwo.Substring(0,EndIndexTwo);
                return $"{str1} & {str2}";
            }
        }


    }

    public class DateAttribute : RangeAttribute
    {
            public DateAttribute()
            : base(typeof(DateTime), 
                DateTime.Now.ToString("MM/dd/yyyy"),     
                DateTime.Now.AddYears(100).ToString("MM/dd/yyyy")) 
            { } 
    }
}
