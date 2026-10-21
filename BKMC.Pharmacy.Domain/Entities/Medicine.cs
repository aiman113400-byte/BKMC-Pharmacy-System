using System;
using System.Collections.Generic;
using System.Text;

namespace BKMC.Pharmacy.Domain.Entities
{
    
    public class Medicine
    {
        public int Id { get; set; }

        public string MedicineCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string GenericName { get; set; } = string.Empty;

        public string Strength { get; set; } = string.Empty;

        public string Form { get; set; } = string.Empty;
        public decimal ReorderLevel { get; set; } = 0;                    //add new code here//

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
