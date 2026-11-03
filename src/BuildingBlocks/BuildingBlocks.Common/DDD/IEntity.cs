namespace BuildingBlocks.Common.DDD
{
    public interface IEntity<T> : IEntity
    {
        public T Id { get; set; }
    }

    public interface IEntity
    {
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}