namespace BuildingBlocks.Common.DDD
{
    public abstract class Entity<T> : IEntity<T>
    {
        public T Id { get; set; } = default!;
        public bool IsActive { get; protected set; } = true;
        public bool IsDeleted { get; protected set; }
        public DateTime? CreatedAt { get; protected set; }
        public string? CreatedBy { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string? UpdatedBy { get; protected set; }
    }
}