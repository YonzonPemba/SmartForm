using SmartForm.Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Repository.Repositories
{
    public class EFRepository<T> : IRepository<T> where T : class
    {
        protected IUnitOfWork unitOfWork;

        protected readonly DbContext dbContext;
        protected readonly ObjectContext objectContext;
        protected readonly DbSet<T> dbSet;

        public EFRepository(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

            this.dbContext = unitOfWork.DbContext;
            this.objectContext = unitOfWork.ObjectContext;
            this.dbSet = this.dbContext.Set<T>();
        }

        public virtual int Count
        {
            get
            {
                return this.dbSet.Count();
            }
        }

        public virtual T LoadByID(object id)
        {
            return this.dbSet.Find(id);
        }

        public virtual IQueryable<T> Load()
        {
            return this.dbSet.AsQueryable();
        }

        public virtual IQueryable<T> Load(int pageIndex, int pageSize)
        {
            var query = this.Load();

            query = ApplyPageFilter(query, pageIndex, pageSize);

            return query.AsQueryable();
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate)
        {
            return this.dbSet.Where(predicate).AsQueryable();
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad)
        {
            var query = this.Load(predicate);

            query = IncludeEntities(query, entitiesToLoad);

            return query.AsQueryable();
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize)
        {
            var query = this.Load(predicate);

            query = ApplyPageFilter(query, pageIndex, pageSize);

            return query.AsQueryable();
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad, int pageIndex, int pageSize)
        {
            var query = this.Load(predicate);

            query = IncludeEntities(query, entitiesToLoad);
            query = ApplyPageFilter(query, pageIndex, pageSize);

            return query.AsQueryable();
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return this.dbSet.Count(predicate) > 0;
        }

        public virtual T Add(T entity)
        {
            return this.dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            var updatedEntity = this.dbContext.Entry(entity);
            this.dbSet.Attach(entity);
            updatedEntity.State = System.Data.Entity.EntityState.Modified;
        }

        public virtual void DeleteByID(object id)
        {
            var entityToDelete = this.LoadByID(id);

            Delete(entityToDelete);
        }

        public virtual void DeleteByID(object id, bool deletePhysicalRecord)
        {
            var entityToDelete = this.LoadByID(id);

            Delete(entityToDelete, true);
        }

        public virtual void Delete(T entity)
        {
            Delete(entity, false);
        }

        public virtual void Delete(T entity, bool deletePhysicalRecord)
        {
            if (deletePhysicalRecord)
            {
                if (this.dbContext.Entry(entity).State == System.Data.Entity.EntityState.Detached)
                {
                    this.dbSet.Attach(entity);
                }

                this.dbSet.Remove(entity);
            }
        }

        public virtual void Delete(Expression<Func<T, bool>> predicate)
        {
            Delete(predicate, false);
        }

        public virtual void Delete(Expression<Func<T, bool>> predicate, bool deletePhysicalRecord)
        {
            var entitiesToDelete = this.Load(predicate);

            foreach (var entityToDelete in entitiesToDelete)
            {
                Delete(entityToDelete, deletePhysicalRecord);
            }
        }

        public virtual IEnumerable<V> ExecuteFunction<V>(string functionName) where V : class
        {
            ObjectParameter[] objectParameters = new ObjectParameter[] { };
            return ExecuteFunction<V>(functionName, objectParameters);
        }

        public virtual IEnumerable<V> ExecuteFunction<V>(string functionName, Dictionary<string, object> parameters) where V : class
        {
            ObjectParameter[] objectParameters = parameters.Select(p => new ObjectParameter(p.Key, p.Value != null ? p.Value.ToString().Replace("'", "''").Replace("\"", "\"\"") : p.Value)).ToArray();
            return ExecuteFunction<V>(functionName, objectParameters);
        }

        protected IEnumerable<V> ExecuteFunction<V>(string functionName, ObjectParameter[] objectParameters) where V : class
        {
            return this.objectContext.ExecuteFunction<V>(functionName, objectParameters);
        }

        public virtual IEnumerable<dynamic> ExecuteFunction(string functionName)
        {
            throw new NotImplementedException("Method not implemented. Entity framework does not support non-entities as return types.");
        }

        public virtual IEnumerable<dynamic> ExecuteFunction(string functionName, Dictionary<string, object> parameters)
        {
            throw new NotImplementedException("Method not implemented. Entity framework does not support non-entities as return types.");
        }

        public virtual IEnumerable<V> ExecuteQuery<V>(string query) where V : class
        {
            return this.dbContext.Database.SqlQuery<V>(query);
        }

        public virtual IEnumerable<V> ExecuteQuery<V>(string query, Dictionary<string, object> parameters) where V : class
        {
            throw new NotImplementedException("Method not implemented in this version.");
        }

        public virtual IEnumerable<dynamic> ExecuteQuery(string query)
        {
            return this.ExecuteQuery(query, new Dictionary<string, object>());
        }

        public virtual IEnumerable<dynamic> ExecuteQuery(string query, Dictionary<string, object> parameters)
        {
            using (var command = dbContext.Database.Connection.CreateCommand())
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                if (this.unitOfWork.IsInTransaction) command.Transaction = dbContext.Database.CurrentTransaction.UnderlyingTransaction;
                command.CommandTimeout = 0;
                command.CommandText = query;

                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    DbParameter dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = parameter.Key;
                    dbParameter.Value = parameter.Value;

                    command.Parameters.Add(dbParameter);
                }

                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var dataRow = GetDataRow(dataReader);
                        yield return dataRow;
                    }
                }
            }
        }

        public virtual IEnumerable<dynamic> ExecuteStoredProcedure(string procedureName, Dictionary<string, object> parameters)
        {
            using (var command = dbContext.Database.Connection.CreateCommand())
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                if (this.unitOfWork.IsInTransaction) command.Transaction = dbContext.Database.CurrentTransaction.UnderlyingTransaction;
                command.CommandTimeout = 360;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedureName;

                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    command.Parameters.Add(new SqlParameter((parameter.Key.Substring(0, 1) == "@" ? "" : "@") + parameter.Key, parameter.Value));
                }

                using (var dataReader = command.ExecuteReader())
                {
                    do
                    {
                        DataTable dt = new DataTable();
                        dt.Load(dataReader);
                        yield return dt;
                    } while (!dataReader.IsClosed);
                }
            }
        }

        protected static dynamic GetDataRow(DbDataReader dataReader)
        {
            var dataRow = new ExpandoObject() as IDictionary<string, object>;

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                dataRow.Add(dataReader.GetName(i), dataReader[i]);
            }

            return dataRow;
        }

        public virtual void ExecuteNonQuery(string query)
        {
            using (var command = dbContext.Database.Connection.CreateCommand())
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                if (this.unitOfWork.IsInTransaction) command.Transaction = dbContext.Database.CurrentTransaction.UnderlyingTransaction;
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        #region Private methods

        private IQueryable<T> IncludeEntities(IQueryable<T> query, IEnumerable<string> entitiesToLoad)
        {
            if (entitiesToLoad != null || entitiesToLoad.All(x => string.IsNullOrWhiteSpace(x)))
            {
                foreach (string entityToLoad in entitiesToLoad)
                {
                    query = query.Include(entityToLoad);
                }
            }

            return query;
        }

        private IQueryable<T> ApplyPageFilter(IQueryable<T> query, int pageIndex, int pageSize)
        {
            int skipCount = pageIndex * pageSize;
            return (skipCount == 0) ? query.Take(pageSize) : query.Skip(skipCount).Take(pageSize);
        }

        #endregion
        public void Save()
        {
            //unitOfWork.SaveChanges();
            dbContext.SaveChanges();
        }

    }
}
