namespace BELEPOS.Entity
{
    public class RepairChecklistDto
    {
        //public Guid TicketId { get; set; }
        //public Guid OrderId { get; set; }
        public List<RepairChecklistItemDto> Responses { get; set; } = new();
    }

    public class RepairChecklistItemDto
    {
        public Guid ChecklistId { get; set; }
        public bool Value { get; set; }

        public String? RepairInspection { get; set; }
    }

    public class CancelRepairRequest
    {
        public Guid TicketId { get; set; }
        public Guid OrderId { get; set; }
        public string Cancelreason { get; set; }

    }
}
