using System;
using System.Collections.Generic;
using System.Text;

namespace BKMC.Pharmacy.Application.DTOs.Medicines
{
   

    public class MedicineDto
    {
        public int Id { get; set; }

        public string MedicineCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string GenericName { get; set; } = string.Empty;

        public string Strength { get; set; } = string.Empty;

        public string Form { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

