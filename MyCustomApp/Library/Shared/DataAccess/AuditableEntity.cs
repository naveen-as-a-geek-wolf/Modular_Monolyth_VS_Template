using Shared.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataAccess
{
    public class AuditableEntity : IAuditableEntity
    {
        public virtual int? CreatedBy { get; protected set; }

        public virtual DateTimeOffset CreatedOn { get; protected set; }

        public virtual int? ModifiedBy { get; protected set; }

        public virtual DateTimeOffset? ModifiedOn { get; protected set; }
        [Timestamp]
        public virtual byte[] Rowversion { get; protected set; } = null!;
        public AuditableEntity()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }
    }
}
