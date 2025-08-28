namespace BELEPOS.Entity
{
    public class CategoryDto 
    {

        public Guid CategoryId { get; set; }

        public string? CategoryName { get; set; } = null!;

        public string? Image { get; set; } = null!;

        public Guid? StoreId { get; set; } = Guid.Empty;

        public string? Description { get; set; }
    }


    public class SubCategoryDto : CategoryDto
    {
        public Guid? SubCategoryId { get; set; } = Guid.Empty;
        public string? SubCategoryName { get; set; } = null!;

    }
}
