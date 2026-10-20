using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    public class Notices
    {
        [Key] public int NoticeId { get; set; }
       public string NoticeMessage { get; set; }
    }
}
