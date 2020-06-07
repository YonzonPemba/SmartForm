using SmartForm.Repository.Contracts;
using SmartForm.Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.EntityServices
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        protected readonly IUnitOfWork unitOfWork;

        protected IRepository<T> repository;

        public virtual int Count
        {
            get
            {
                return this.repository.Count;
            }
        }

        public GenericService(IUnitOfWork unitOfWork, IRepository<T> repository)
        {
            this.unitOfWork = unitOfWork;
            this.repository = repository;
        }

        public virtual T LoadByID(object id)
        {
            return this.repository.LoadByID(id);
        }

        public virtual IQueryable<T> Load()
        {
            return this.repository.Load();
        }

        public virtual IQueryable<T> Load(int pageIndex, int pageSize)
        {
            return this.repository.Load(pageIndex, pageSize);
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate)
        {
            return this.repository.Load(predicate);
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad)
        {
            return this.repository.Load(predicate, entitiesToLoad);
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize)
        {
            return this.repository.Load(predicate, pageIndex, pageSize);
        }

        public virtual IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad, int pageIndex, int pageSize)
        {
            return this.repository.Load(predicate, entitiesToLoad, pageIndex, pageSize);
        }

        public IQueryable<T> AddPageFilter(IQueryable<T> query, int pageIndex, int pageSize)
        {
            if (pageIndex > 0 && pageSize > 0)
            {
                query = query.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            }

            return query;
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return this.repository.Exists(predicate);
        }

        public virtual T Add(T entity)
        {
            return this.repository.Add(entity);
        }

        public virtual void Update(T entity)
        {
            this.repository.Update(entity);
        }

        public virtual void DeleteByID(object id)
        {
            this.repository.DeleteByID(id);
        }

        public virtual void DeleteByID(object id, bool deletePhysicalRecord)
        {
            this.repository.DeleteByID(id, deletePhysicalRecord);
        }


        public virtual void Delete(T entity)
        {
            this.repository.Delete(entity);
        }

        public virtual void Delete(T entity, bool deletePhysicalRecord)
        {
            this.repository.Delete(entity, deletePhysicalRecord);
        }

        public virtual void Delete(Expression<Func<T, bool>> predicate)
        {
            this.repository.Delete(predicate);
        }

        public virtual void Delete(Expression<Func<T, bool>> predicate, bool deletePhysicalRecord)
        {
            this.repository.Delete(predicate, deletePhysicalRecord);
        }

        public virtual IEnumerable<dynamic> ExecuteQuery(string query)
        {
            return this.repository.ExecuteQuery(query);
        }

        public virtual IEnumerable<V> ExecuteFunction<V>(string functionName) where V : class
        {
            return this.repository.ExecuteFunction<V>(functionName);
        }

        public virtual IEnumerable<V> ExecuteFunction<V>(string functionName, Dictionary<string, object> parameters) where V : class
        {
            return this.repository.ExecuteFunction<V>(functionName, parameters);
        }

        public virtual IEnumerable<dynamic> ExecuteFunction(string functionName)
        {
            return this.repository.ExecuteFunction(functionName);
        }

        public virtual IEnumerable<dynamic> ExecuteFunction(string functionName, Dictionary<string, object> parameters)
        {
            return this.repository.ExecuteFunction(functionName, parameters);
        }

        public virtual IEnumerable<dynamic> ExecuteStoredProcedure(string procedureName, Dictionary<string, object> parameters)
        {
            return this.repository.ExecuteStoredProcedure(procedureName, parameters);
        }

        public DateTime GetDatabaseTimestamp()
        {
            return this.repository.ExecuteQuery<DateTimeHolder>("SELECT GETDATE() AS CurrentDate").SingleOrDefault().CurrentDate;
        }

        public virtual void ExecuteNonQuery(string query)
        {
            this.repository.ExecuteNonQuery(query);
        }

        public void Save()
        {
            this.repository.Save();
            //unitOfWork.SaveChanges();
        }
        public void Dispose()
        {
            unitOfWork.Dispose();
        }

        internal class DateTimeHolder
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            public DateTimeHolder()
            {
            }

            /// <summary>
            /// Declare a property of DateTime class
            /// </summary>
            public DateTime CurrentDate { get; set; }
        }
    }

}
