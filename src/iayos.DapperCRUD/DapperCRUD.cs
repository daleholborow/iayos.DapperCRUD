using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Diagnostics;
using System.Linq.Expressions;
using System;
using System.Reflection;

namespace iayos.DapperCRUD
{
    public static partial class DapperCRUD
    {

        //      /// <summary>
        ///// If an ambient transaction already exists, subscribe to that db connection, otherwise, create a new connection.
        ///// </summary>
        ///// <param name="transaction"></param>
        ///// <returns></returns>
        //[DebuggerStepThrough]
        //      public static IDbConnection GetOrOpenConnection(IDbTransaction transaction = null)
        //      {
        //          if (transaction != null) return transaction.Connection;

        //          var db = new SqlConnection(_pallasPersistenceSettings.PallasDbConnectionString);
        //          db.Open();
        //          return db;
        //      }

        /// <summary>
        /// SimpleCRUD wraps tables and columns in [ColumnName] but our scripts dont want/need because they do it explicitly
        /// </summary>
        /// <param name="dirty"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        private static string CleanDbElementName(string dirty)
		{
			return dirty.Replace("[", "").Replace("]", "");
		}


		/// <summary>
		/// Gets the table name for this type
		/// For Get(id) and Delete(id) we don't have an entity, just the type so this method is used
		/// Use dynamic type to be able to handle both our Table-attribute and the DataAnnotation
		/// Uses class name by default and overrides if the class has a Table attribute
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static string GetCleanTableName(Type type)
		{
			var dirtyTableName = GetTableName(type);
			return CleanDbElementName(dirtyTableName);
		}


        /// <summary>
		/// Dale method, added
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[DebuggerStepThrough]
        private static string GetCleanTableName<T>()
        {
            Type type = typeof(T);
            object obj1 = Activator.CreateInstance(type);
            var tableName = GetCleanTableName(obj1);
            return tableName;
        }


        /// <summary>
		/// Gets the table name for this entity
		/// For Inserts and updates we have a whole entity so this method is used
		/// Uses class name by default and overrides if the class has a Table attribute
		/// https://github.com/ericdc1/Dapper.SimpleCRUD/blob/master/Dapper.SimpleCRUD/SimpleCRUD.cs
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
        public static string GetCleanTableName(object entity)
        {
            var type = entity.GetType();
            var tableName = GetCleanTableName(type);
            return CleanDbElementName(tableName);
        }


        public static List<TDesired> GetXsWhereYIn<TTable, TDesired, TIn>(
			this IDbConnection connection,
			string tableName,
			string desiredColumnName,
			string inColumnName,
			ICollection<TIn> inValues,
			IDbTransaction transaction = null
			)
		{
			if (inValues.Any() == false) return new List<TDesired>();
			inValues = inValues.Distinct().ToList();
			tableName = CleanDbElementName(tableName);
			inColumnName = CleanDbElementName(inColumnName);
			desiredColumnName = CleanDbElementName(desiredColumnName);

			List<TDesired> results;
			
			var parameters = new { InValues = inValues };
			var query = $"SELECT [{desiredColumnName}] FROM [{tableName}] WHERE [{inColumnName}] IN @InValues";
			results = connection.Query<TDesired>(query, parameters, transaction).ToList();
			return results;
		}


		/// <summary>
        /// Usage: 
        /// var names = new List<string> { "dale", "ronaldo", "sha'niqua" };
        /// var ids = db.GetXWhereYIn<User, long, string>(u => u.UserId, u => u.FirstName, names);
		/// Select a multiple rows of a single column, where some other column (typically an ID column, 
		/// but not necessarily) is IN the specified values collection.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TDesired"></typeparam>
		/// <typeparam name="TIn"></typeparam>
		/// <param name="desiredPropertyRefExpr"></param>
		/// <param name="inPropertyRefExpr"></param>
		/// <param name="inValues"></param>
		/// <param name="selectActiveOnly"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static List<TDesired> GetXsWhereYIn<TEntity, TDesired, TIn>(
            this IDbConnection connection,
            Expression<Func<TEntity, TDesired>> desiredPropertyRefExpr,
			Expression<Func<TEntity, TIn>> inPropertyRefExpr,
			ICollection<TIn> inValues,
			IDbTransaction transaction = null)
		{
			var tableName = GetCleanTableName<TEntity>();
			var desiredColumnName = PropertyHelper.GetName(desiredPropertyRefExpr);
			var equalsColumnName = PropertyHelper.GetName(inPropertyRefExpr);
			return connection.GetXsWhereYIn<TEntity, TDesired, TIn>(tableName, desiredColumnName, equalsColumnName, inValues, transaction);
		}


        /// <summary>
        /// Select a multiple rows of a single column, where some other column (typically an ID column, 
        /// but not necessarily) equals the specified value.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TDesired"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="desiredColumnName"></param>
        /// <param name="equalsColumnName"></param>
        /// <param name="exactValue"></param>
        /// <param name="selectActiveOnly"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static List<TDesired> GetXsWhereYEquals<TEntity, TDesired>(
            this IDbConnection connection,
            string tableName,
            string desiredColumnName,
            string equalsColumnName,
            object exactValue,
            IDbTransaction transaction = null)
        {
            tableName = CleanDbElementName(tableName);
            equalsColumnName = CleanDbElementName(equalsColumnName);
            List<TDesired> results;
            var parameters = new { ExactValue = exactValue };
            var query = $"SELECT [{desiredColumnName}] FROM [{tableName}] WHERE [{equalsColumnName}] = @ExactValue";
            results = connection.Query<TDesired>(query, parameters, transaction).ToList();
            return results;
        }


        [DebuggerStepThrough]
        public static List<TDesired> GetXsWhereYEquals<TEntity, TDesired>(
            this IDbConnection connection,
            Expression<Func<TEntity, TDesired>> desiredPropertyRefExpr,
            Expression<Func<TEntity, object>> equalsPropertyRefExpr,
            object exactValue,
            IDbTransaction transaction = null)
        {
            var tableName = GetCleanTableName<TEntity>();
            var desiredColumnName = PropertyHelper.GetName(desiredPropertyRefExpr);
            var equalsColumnName = PropertyHelper.GetName(equalsPropertyRefExpr);
            return connection.GetXsWhereYEquals<TEntity, TDesired>(tableName, desiredColumnName, equalsColumnName, exactValue, transaction);
        }


        /// <summary>
		/// Search an single column within a table by a particular Column name applying the LIKE 
		/// condition (any % wildcards must be applied to search term PRIOR to use of this method)
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TColumn"></typeparam>
		/// <param name="tableName"></param>
		/// <param name="desiredColumnName"></param>
		/// <param name="likeColumnName"></param>
		/// <param name="likeValue"></param>
		/// <param name="selectActiveOnly"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
        public static List<TColumn> GetXsWhereYLike<TEntity, TColumn>(
            this IDbConnection connection,
            string tableName,
            string desiredColumnName,
            string likeColumnName,
            string likeValue,
            IDbTransaction transaction = null)
        {
            tableName = CleanDbElementName(tableName);
            desiredColumnName = CleanDbElementName(desiredColumnName);
            likeColumnName = CleanDbElementName(likeColumnName);
            List<TColumn> results;
            var parameters = new { LikeValue = likeValue };
            var query = $"SELECT [{desiredColumnName}] FROM [{tableName}] WHERE [{likeColumnName}] LIKE @LikeValue";
            results = connection.Query<TColumn>(query, parameters, transaction).ToList();
            return results;
        }


        /// <summary>
		/// Select a multiple rows of a single column, where some other column (typically an ID column, 
		/// but not necessarily) is LIKE the specified value.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TDesired"></typeparam>
		/// <param name="desiredPropertyRefExpr"></param>
		/// <param name="likePropertyRefExpr"></param>
		/// <param name="likeValue"></param>
		/// <param name="selectActiveOnly"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
        public static List<TDesired> GetXsWhereYLike<TEntity, TDesired>(
            this IDbConnection connection,
            Expression<Func<TEntity, TDesired>> desiredPropertyRefExpr,
            Expression<Func<TEntity, object>> likePropertyRefExpr,
            string likeValue,
            IDbTransaction transaction = null)
        {
            var tableName = GetCleanTableName<TEntity>();
            var desiredColumnName = PropertyHelper.GetName(desiredPropertyRefExpr);
            var likeColumnName = PropertyHelper.GetName(likePropertyRefExpr);
            return connection.GetXsWhereYLike<TEntity, TDesired>(tableName, desiredColumnName, likeColumnName, likeValue, transaction);
        }



        /// <summary>
		/// Search Entities by a particular Column name where that column IN some collection of values
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <typeparam name="TIn"></typeparam>
		/// <param name="tableName"></param>
		/// <param name="columnName"></param>
		/// <param name="inValues"></param>
		/// <param name="selectActiveOnly"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
        public static List<TEntity> GetEntitiesWhereYIn<TEntity, TIn>(
            this IDbConnection connection,
            string tableName,
            string columnName,
            IEnumerable<TIn> inValues,
            bool selectActiveOnly = true,
            IDbTransaction transaction = null
        )
        {
            if (inValues.Any() == false) return new List<TEntity>();
            inValues = inValues.Distinct().ToList();
            tableName = CleanDbElementName(tableName);
            columnName = CleanDbElementName(columnName);

            List<TEntity> results;
            var parameters = new { InValues = inValues };
            var query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] IN @InValues";
            results = connection.Query<TEntity>(query, parameters, transaction).ToList();
            return results;
        }


        /// <summary>
		/// Search an Entity by a particular Column name applying the LIKE condition (any % wildcards must be applied to search term PRIOR to use of this method)
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="tableName"></param>
		/// <param name="columnName"></param>
		/// <param name="likeValue"></param>
		/// <param name="selectActiveOnly"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
        public static List<TEntity> GetEntitiesWhereYLike<TEntity>(
            this IDbConnection connection,
            string tableName,
            string columnName,
            string likeValue,
            bool selectActiveOnly = true,
            IDbTransaction transaction = null)
        {
            tableName = CleanDbElementName(tableName);
            columnName = CleanDbElementName(columnName);
            List<TEntity> results;
            var parameters = new { LikeValue = likeValue };
            var query = $"SELECT * FROM [{tableName}] WHERE [{columnName}] LIKE @LikeValue";
            results = connection.Query<TEntity>(query, parameters, transaction).ToList();
            return results;
        }


    }


    /// <summary>
	/// How to use nice short type-safe property names on generic classes
	/// http://ivanz.com/2009/12/04/how-to-avoid-passing-property-names-as-strings-using-c-3-0-expression-trees/
	/// </summary>
	public static class PropertyHelper
    {

        /// <summary>
        /// Get property name for an INSTANCE:
        /// e.g. 
        /// User user = new User();
        /// string propertyName = user.GetPropertyName (u =&gt; u.Email);
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="type"></param>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetPropertyName<TObject>(this TObject type, Expression<Func<TObject, object>> propertyRefExpr)
        {
            return GetPropertyNameCore(propertyRefExpr.Body);
        }


        /// <summary>
        /// Get property name for a TYPE:
        /// e.g. string propertyName = PropertyUtil.GetName&lt;User&gt; (u =&gt; u.Email);
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetName<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
        {
            return GetPropertyNameCore(propertyRefExpr.Body);
        }

        /// <summary>
        /// Get property name for a TYPE:
        /// e.g. string propertyName = PropertyUtil.GetName&lt;User&gt; (u =&gt; u.Email);
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetName<TObject, TProperty>(Expression<Func<TObject, TProperty>> propertyRefExpr)
        {
            return GetPropertyNameCore(propertyRefExpr.Body);
        }


        [DebuggerStepThrough]
        private static string GetPropertyNameCore(Expression propertyRefExpr)
        {
            if (propertyRefExpr == null) throw new ArgumentNullException("propertyRefExpr", "propertyRefExpr is null.");

            MemberExpression memberExpr = propertyRefExpr as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = propertyRefExpr as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert) memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property) return memberExpr.Member.Name;

            throw new ArgumentException("No property reference expression was found.", "propertyRefExpr");
        }

    }
}
