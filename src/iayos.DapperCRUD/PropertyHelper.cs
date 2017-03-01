using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace iayos.DapperCRUD
{

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
