using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Repository.Contracts
{
   public interface IUnitOfWork:IDisposable
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        DbContext DbContext { get; }

        /// <summary>
        /// Gets the object context from db context.
        /// </summary>        
        ObjectContext ObjectContext { get; }

        /// <summary>
        /// Saves changes.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Saves changes as per specified options.
        /// </summary>
        /// <param name="saveOptions">Save options</param>
        void SaveChanges(SaveOptions saveOptions);

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Starts a transaction with isolation level.
        /// </summary>
        /// <param name="isolationLevel">Isolation level</param>
        void BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Gets if transaction is active.
        /// </summary>
        bool IsInTransaction { get; }

        /// <summary>
        /// Rolls back a transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Releases Current Transaction
        /// </summary>
        void ReleaseCurrentTransaction();

        /// <summary>
        /// Commits a transaction.
        /// </summary>
        void CommitTransaction();
    }
}
