using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartForm.Repository.Contracts
{
   public interface IRepository<T> where T:class
    {
        int Count { get; }

        T LoadByID(object id);

        IQueryable<T> Load();

        IQueryable<T> Load(int pageIndex, int pageSize);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize);

        IQueryable<T> Load(Expression<Func<T, bool>> predicate, IEnumerable<string> entitiesToLoad, int pageIndex, int pageSize);

        bool Exists(Expression<Func<T, bool>> predicate);

        T Add(T entity);

        void Update(T entity);

        void DeleteByID(object id);

        void DeleteByID(object id, bool deletePhysicalRecord);

        void Delete(T entity);

        void Delete(T entity, bool deletePhysicalRecord);

        void Delete(Expression<Func<T, bool>> predicate);

        void Delete(Expression<Func<T, bool>> predicate, bool deletePhysicalRecord);

        IEnumerable<V> ExecuteFunction<V>(string functionName) where V : class;

        IEnumerable<V> ExecuteFunction<V>(string functionName, Dictionary<string, object> parameters) where V : class;

        IEnumerable<dynamic> ExecuteFunction(string functionName);

        IEnumerable<dynamic> ExecuteFunction(string functionName, Dictionary<string, object> parameters);

        IEnumerable<dynamic> ExecuteStoredProcedure(string procedureName, Dictionary<string, object> parameters);

        IEnumerable<V> ExecuteQuery<V>(string query) where V : class;

        IEnumerable<V> ExecuteQuery<V>(string query, Dictionary<string, object> parameters) where V : class;

        IEnumerable<dynamic> ExecuteQuery(string query);

        IEnumerable<dynamic> ExecuteQuery(string query, Dictionary<string, object> parameters);

        void ExecuteNonQuery(string query);

        void Save();

    }
}
