using System.ComponentModel.DataAnnotations;

namespace BELEPOS.Entity
{
    public class RepairTicketsDto
    {

        public Guid Ticketid { get; set; }
        public string? DeviceType { get; set; }
        public string IPAddress { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ImeiNumber { get; set; }
        public string? SerialNumber { get; set; }
        public string? Passcode { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal RepairCost { get; set; }
        public Guid? TechnicianId { get; set; }

        public string? DeviceColour { get; set; }   

        public string? TechnicianName { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public Guid? TaskTypeId { get; set; }

        public bool? Isfinalsubmit { get; set; }

        public List<string>? Contactmethod { get; set; } = new();


        public Guid? CustomerId { get; set; }

        public Guid? RepairOrderId { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerNumber { get; set; }


        public Guid? UserId { get; set; }

        public String? OrderNumber { get; set; }

        public String TicketNo { get; set; }

        public DateTime? CreatedAt { get; set; }


        public string? TaskTypeName { get; set; }

        public List<TicketsNotesDto>? Notes { get; set; }

        public List<RepairOrderPartDto>? OrderParts { get; set; }
    }

    public class TicketsNotesDto
    {
        public Guid Id { get; set; }
        public string Notes { get; set; }
        public string Type { get; set; }
    }
}
