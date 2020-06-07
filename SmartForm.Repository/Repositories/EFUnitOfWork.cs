using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Repository.Repositories
{
    public class EFUnitOfWork : IUnitOfWork, IDisposable
    {
        /// <summary>
        /// The context.
        /// </summary>
        private DbContext dbContext;

        /// <summary>
        /// Transaction object
        /// </summary>
        private DbTransaction transaction;

        /// <summary>
        /// Status of disposal
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Gets the context.
        /// </summary>        
        public DbContext DbContext
        {
            get
            {
                return this.dbContext;
            }
        }

        /// <summary>
        /// Gets the object context from db context.
        /// </summary>        
        public ObjectContext ObjectContext
        {
            get
            {
                return ((IObjectContextAdapter)this.dbContext).ObjectContext;
            }
        }

        /// <summary>
        /// Initializes a new instance of the class UnitOfWork.
        /// </summary>
        /// <param name="context">Database context</param>
        public EFUnitOfWork(IDbContextFactory contextFactory)
        {
            this.dbContext = contextFactory.GetContext();

            if (this.dbContext == null)
            {
                throw new ArgumentNullException("context");
            }
        }


        //public void SaveChanges()
        //{
        //    if (((IObjectContextAdapter)this.dbContext).ObjectContext.Connection.State == ConnectionState.Closed)
        //    {
        //        ((IObjectContextAdapter)this.dbContext).ObjectContext.Connection.Open();
        //    }

        //    ((IObjectContextAdapter)this.dbContext).ObjectContext.ExecuteStoreCommand("exec spSetUserContext @userInfo = {0}", ContextInfo.ModuleName + (string.IsNullOrEmpty(ContextInfo.UserId) ? string.Empty : ("/" + ContextInfo.UserId)));

        //    Action<DbContext> inlineAction = (sourceDbContext) =>
        //    {
        //        ((IObjectContextAdapter)sourceDbContext).ObjectContext.SaveChanges();
        //    };
        //}



        public void SaveChanges()
        {
            dbContext.SaveChanges();
        }


        /// <summary>
        /// Saves changes as per specified options.
        /// </summary>
        /// <param name="saveOptions">Save options.</param>
        public void SaveChanges(SaveOptions saveOptions)
        {
            if (this.IsInTransaction)
            {
                throw new ApplicationException("A transaction is active. Use CommitTransaction to save changes.");
            }

            ObjectContext.SaveChanges(saveOptions);
        }

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        public void BeginTransaction()
        {
            this.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Starts a transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">isolation level</param>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (this.transaction != null)
            {
                throw new ApplicationException("A transaction is already active. Commit or rollback the existing transaction before starting a new one.");
            }

            // Opens a new connection
            this.OpenConnection();

            // Start a new transaction with the specified isolation level.
            this.transaction = ObjectContext.Connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Gets if transaction is active.
        /// </summary>
        public bool IsInTransaction
        {
            get
            {
                return this.transaction != null;
            }
        }

        /// <summary>
        /// Rolls back a transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            if (this.transaction != null && this.transaction.Connection != null)
                this.transaction.Rollback();

            this.ReleaseCurrentTransaction();
        }

        /// <summary>
        /// Commits a transaction.
        /// </summary>
        public void CommitTransaction()
        {
            if (this.transaction == null)
            {
                throw new ApplicationException("Cannot rollback a transaction. There is no active transaction.");
            }

            try
            {
                // Saves context changes and commit transaction.
                this.SaveChanges();
                if (this.transaction.Connection != null)
                    this.transaction.Commit();
            }
            catch
            {
                // Discards changes and rolls back transaction.
                this.RollbackTransaction();
                throw new ApplicationException("Error occured during commit transaction. Transaction is rolled back.");
            }
            finally
            {
                this.ReleaseCurrentTransaction();
            }
        }

        /// <summary>
        /// Disposes (releases resources).
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        private void OpenConnection()
        {
            if (ObjectContext.Connection.State != ConnectionState.Open)
            {
                ObjectContext.Connection.Open();
            }
        }

        /// <summary>
        /// Releases the current transaction.
        /// </summary>
        public void ReleaseCurrentTransaction()
        {
            if (this.transaction != null)
            {
                this.transaction.Dispose();
                this.transaction = null;
            }
        }

        /// <summary>
        /// Dispose process.
        /// </summary>
        /// <param name="disposing">if class disposing</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (dbContext != null)
            {
                if (this.dbContext.Database.Connection.State == ConnectionState.Open)
                {
                    this.dbContext.Database.Connection.Close();
                }

                this.dbContext.Dispose();
                this.dbContext = null;
            }

            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }
    }

}