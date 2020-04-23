using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
public class UserWedWrapper
    {
        public User ThisUser {get;set;}
        public List<Wedding> EveryWedding {get;set;}

    }
}