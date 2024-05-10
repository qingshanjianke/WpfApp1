using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using WpfApp1.Common;
using WpfApp1.Models;

namespace WpfApp1.Database
{
    [Export]
    public class DbContext : SqlSugarScope
    {
        public DbContext(ILogger<DbContext> logger, ApplicationConfig applicationConfig, Action<SqlSugarClient>? configAction = null) : base(new ConnectionConfig()
        {
            ConnectionString = "Data Source=" + applicationConfig.ProgramDataPath + "\\Data.db",
            DbType = DbType.Sqlite,//设置数据库类型
            IsAutoCloseConnection = true,//自动释放数据库，如果存在事务，在事务结束之后释放。
            InitKeyType = InitKeyType.Attribute,//从实体特性中读取主键自增列信息
        }, a =>
        {
            Stopwatch stopwatch = null;
            a.Aop.OnLogExecuting = (sql, paramters) =>
            {
                stopwatch = Stopwatch.StartNew();
            };
            a.Aop.OnLogExecuted = (sql, paramters) =>
            {
                sql = string.Join(" ", sql.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
                foreach (var item in paramters.OrderByDescending(q => q.ParameterName))
                {
                    sql = sql.Replace(item.ParameterName, $"'{item.Value}'");
                }
                logger.LogDebug($"cost:{stopwatch.ElapsedMilliseconds},{sql}");
            };
            a.Aop.OnError = (a) =>
            {
                a.Sql = string.Join(" ", a.Sql.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
                foreach (var item in ((SugarParameter[])a.Parametres).OrderByDescending(q => q.ParameterName))
                {
                    a.Sql = a.Sql.Replace(item.ParameterName, $"'{item.Value}'");
                }
                logger.LogError($"{a.Sql},{a}");
            };
            configAction?.Invoke(a);
        })
        {
        }
        public void InitTables(List<Assembly> assemblyList)
        {
            var types = assemblyList.Select(q => q.GetTypes()).SelectMany(q => q).Where(q => typeof(ICreateTable).IsAssignableFrom(q) && !q.IsAbstract).ToArray();
            this.CodeFirst.InitTables(types);
        }
        public DbSet<Plu> GlobalSetting => new(this);
        //public DbSet<PhoneGroup> PhoneGroup => new(this);
        //public DbSet<Phone> Phone => new(this);
        //public DbSet<DeviceBattery> DeviceBattery => new(this);
        //public DbSet<Settings> Settings => new(this);
    }



    public class DbSet<T> : SimpleClient<T> where T : class, new()
    {
        private new ISqlSugarClient Context => base.Context.CopyNew();

        public DbSet(SqlSugarScope context) : base(context)
        {

        }

        public new T Insert(T entity)
        {
            Context.Insertable(entity).ExecuteReturnEntity();
            return entity;
        }
        public new async Task InsertRangeAsync(List<T> entity)
        {
            await Context.Insertable(entity).ExecuteCommandAsync();
        }
        public int InsertRangeCount(HashSet<T> list)
        {
            return Context.Insertable(list.ToArray()).ExecuteCommand();
        }
        public int InsertCount(List<T> list)
        {
            return Context.Insertable(list.ToArray()).ExecuteCommand();
        }

        public int InsertCount(T insertObj)
        {
            return Context.Insertable(insertObj).ExecuteCommand();
        }

        public int UpdateRangeCount(List<T> updateObjs)
        {
            return Context.Updateable(updateObjs).ExecuteCommand();
        }

        public int UpdateCount(T updateObj)
        {
            return Context.Updateable(updateObj).ExecuteCommand();
        }

        public int UpdateCount(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return Context.Updateable<T>().SetColumns(columns).Where(whereExpression).ExecuteCommand();
        }

        public int DeleteCount(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Deleteable<T>().Where(whereExpression).ExecuteCommand();
        }

        public int UpdateSetColumnsTrueCount(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return Context.Updateable<T>().SetColumns(columns, appendColumnsByDataFilter: true).Where(whereExpression).ExecuteCommand();
        }
        public new void InsertOrUpdate(T entity)
        {
            Context.Saveable(entity).ExecuteReturnEntity();
        }
        public new async Task InsertOrUpdateAsync(T entity)
        {
            await Context.Saveable(entity).ExecuteReturnEntityAsync();
        }
        public new async Task<int> InsertOrUpdateAsync(List<T> entity)
        {
            return await Context.Saveable(entity).ExecuteCommandAsync();
        }
        public new async Task<int> InsertOrUpdateCountAsync(T entity)
        {
            return await Context.Saveable(entity).ExecuteCommandAsync();
        }

        public ISugarQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return Context.Queryable<T>().Where(expression);
        }

        public ISugarQueryable<T> OrderBy(Expression<Func<T, object>> expression)
        {
            return Context.Queryable<T>().OrderBy(expression);
        }

        public ISugarQueryable<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            return Context.Queryable<T>().OrderBy(expression, OrderByType.Desc);
        }

        public ISugarQueryable<T, T2> InnerJoin<T2>(DbSet<T2> joinTable, Expression<Func<T, T2, bool>> joinExpression) where T2 : class, new()
        {
            return Context.Queryable<T>().InnerJoin(Context.Queryable<T2>(), joinExpression);
        }

        public ISugarQueryable<T, T2> LeftJoin<T2>(DbSet<T2> joinTable, Expression<Func<T, T2, bool>> joinExpression) where T2 : class, new()
        {
            return Context.Queryable<T>().LeftJoin(Context.Queryable<T2>(), joinExpression);
        }

        public ISugarQueryable<T> WhereIF(bool isWhere, Expression<Func<T, bool>> expression)
        {
            return Context.Queryable<T>().WhereIF(isWhere, expression);
        }
        public new virtual Task<T> GetFirstAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().FirstAsync(whereExpression);
        }

        public new virtual Task<T> GetByIdAsync(dynamic id)
        {
            return Context.Queryable<T>().InSingleAsync(id);
        }
        public new virtual async Task InsertAsync(T insertObj)
        {
            await Context.Insertable(insertObj).ExecuteReturnEntityAsync();
        }
        public new virtual Task<bool> IsAnyAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).AnyAsync();
        }
        public new virtual Task<int> CountAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).CountAsync();
        }
        public new virtual async Task<bool> UpdateAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return await Context.Updateable<T>().SetColumns(columns).Where(whereExpression).ExecuteCommandAsync() > 0;
        }

        public new T GetFirst(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().First(whereExpression);
        }

        public new bool IsAny(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Any();
        }

        public new bool Update(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return Context.Updateable<T>().SetColumns(columns).Where(whereExpression)
                .ExecuteCommand() > 0;
        }

        public new bool Delete(T deleteObj)
        {
            return Context.Deleteable<T>().Where(deleteObj).ExecuteCommand() > 0;
        }

        public new bool Update(T updateObj)
        {
            return Context.Updateable(updateObj).ExecuteCommand() > 0;
        }

        public new async Task<bool> DeleteByIdsAsync(dynamic[] ids)
        {
            return await Context.Deleteable<T>().In(ids).ExecuteCommandAsync() > 0;
        }
        public new virtual async Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Context.Deleteable<T>().Where(whereExpression).ExecuteCommandAsync() > 0;
        }

        public new virtual async Task<bool> UpdateAsync(T updateObj)
        {
            return await Context.Updateable(updateObj).ExecuteCommandAsync() > 0;
        }
    }
}
