using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Service.Contracts
{
    public interface IGenericService<T> where T : class
    {
        int Count { get; }

        T LoadByID(object id);

        IQueryable<T> Load();

        IQueryable<T> Load(int pageIndex, int pageSize);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad, int pageIndex, int pageSize);

        IQueryable<T> AddPageFilter(IQueryable<T> query, int pageIndex, int pageSize);

        bool Exists(Expression<Func<T, bool>> predicate);

        T Add(T entity);

        void Update(T entity);

        void DeleteByID(object id);

        void DeleteByID(object id, bool deletePhysicalRecord);

        void Delete(T entity);

        void Delete(T entity, bool deletePhysicalRecord);

        void Delete(Expression<Func<T, bool>> predicate);

        void Delete(Expression<Func<T, bool>> predicate, bool deletePhysicalRecord);

        IEnumerable<dynamic> ExecuteQuery(string query);

        IEnumerable<V> ExecuteFunction<V>(string functionName) where V : class;

        IEnumerable<V> ExecuteFunction<V>(string functionName, Dictionary<string, object> parameters) where V : class;

        IEnumerable<dynamic> ExecuteFunction(string functionName);

        IEnumerable<dynamic> ExecuteFunction(string functionName, Dictionary<string, object> parameters);

        IEnumerable<dynamic> ExecuteStoredProcedure(string procedureName, Dictionary<string, object> parameters);

        DateTime GetDatabaseTimestamp();

        void ExecuteNonQuery(string query);

        void Save();
        void Dispose();
    }
}
