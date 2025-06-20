using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Singular_Systems_SelfKiosk_Software.Models;

namespace SingularSystems_SelfKiosk_Software.Models
{
    public class Wallet
    {   
        public int Id { get; set; } 
        public  int UserId{ get; set; } 
        public int CustomerTransactionId { get; set; }
        public decimal Balance { get; set; }
        //Navigation property
        public User User { get; set; }
        public required ICollection<CustomerTransaction> CustomerTransactions { get; set; }  
 
    }
}
