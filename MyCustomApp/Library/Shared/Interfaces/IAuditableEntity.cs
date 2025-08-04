namespace Shared.Interfaces
{
    internal interface IAuditableEntity
    {
        int? CreatedBy { get; }

        DateTimeOffset CreatedOn { get; }

        int? ModifiedBy { get; }

        DateTimeOffset? ModifiedOn { get; }

        byte[] Rowversion { get; }
    }
}
