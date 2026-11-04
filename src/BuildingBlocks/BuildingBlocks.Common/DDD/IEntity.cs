namespace BuildingBlocks.Common.DDD
{
    public interface IEntity<T> : IEntity
    {
        public T Id { get; }
    }

    public interface IEntity
    {
        public bool IsActive { get; }
        public bool IsDeleted { get; }
        public DateTime? CreatedAt { get; }
        public string? CreatedBy { get; }
        public DateTime? UpdatedAt { get; }
        public string? UpdatedBy { get; }
    }
}