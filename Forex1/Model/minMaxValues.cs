using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forex1.Model
{
    public class minMaxValues
    {
        [Key]
        public int ValueId { get; set; }
        public string ValueName { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }
}
