﻿using Meta.Common.DbHelper;
using Meta.Common.Interface;
using Meta.Common.Model;
using Meta.Common.SqlBuilder.AnalysisExpression;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
namespace Meta.Common.SqlBuilder
{
	public class UpdateBuilder<TModel> : WhereBuilder<UpdateBuilder<TModel>, TModel> where TModel : IDbModel, new()
	{
		/// <summary>
		/// 设置列表
		/// </summary>
		readonly List<string> _setList = new List<string>();
		/// <summary>
		/// 是否返回实体类
		/// </summary>
		bool _isReturn = false;
		public int Count => _setList.Count;

		#region Contructor
		public UpdateBuilder(string fields) : base() => Fields = fields;
		public UpdateBuilder(string table, string alias) : base(table, alias) { }
		public UpdateBuilder() : base(EntityHelper.GetTableName<TModel>()) { }

		#endregion
		private UpdateBuilder<TModel> AddSetExpression(string exp)
		{
			_setList.Add(exp);
			return this;
		}

		/// <summary>
		/// 设置字段等于SQL
		/// </summary>
		/// <param name="selector">key selector</param>
		/// <param name="sqlBuilder">SQL语句</param>
		/// <returns></returns>
		public UpdateBuilder<TModel> Set(Expression<Func<TModel, object>> selector, [DisallowNull]ISqlBuilder sqlBuilder)
		{
			var exp = string.Concat(SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText, " = ", $"({sqlBuilder.CommandText})");
			AddParameters(sqlBuilder.Params);
			return AddSetExpression(exp);
		}

		/// <summary>
		/// 设置一个字段值(非空类型) *可选重载
		/// </summary>
		/// <typeparam name="TKey">字段类型</typeparam>
		/// <param name="isSet">是否设置</param>
		/// <param name="selector">字段key selector</param>
		/// <param name="value">value</param>
		/// <returns></returns>
		public UpdateBuilder<TModel> Set<TKey>(bool isSet, Expression<Func<TModel, TKey>> selector, TKey value) => isSet ? Set(selector, value) : this;

		/// <summary>
		/// 设置一个字段值(可空类型) *可选重载
		/// </summary>
		/// <typeparam name="TKey">字段类型</typeparam>
		/// <param name="isSet">是否设置</param>
		/// <param name="selector">字段key selector</param>
		/// <param name="value">value</param>
		/// <returns></returns>
		public UpdateBuilder<TModel> Set<TKey>(bool isSet, Expression<Func<TModel, TKey?>> selector, TKey? value) where TKey : struct => isSet ? Set(selector, value) : this;

		/// <summary>
		/// 设置一个字段值(非空类型)
		/// </summary>
		/// <typeparam name="TKey">字段类型</typeparam>
		/// <param name="selector">字段key selector</param>
		/// <param name="value">value</param>
		/// <returns></returns>
		public UpdateBuilder<TModel> Set<TKey>(Expression<Func<TModel, TKey>> selector, TKey value)
		{
			var field = SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText;
			if (value == null)
				return AddSetExpression(string.Format("{0} = null", field));

			AddParameterT(value, out string valueIndex);
			return AddSetExpression(string.Format("{0} = @{1}", field, valueIndex));
		}

		/// <summary>
		/// 设置整型等于一个枚举
		/// </summary>
		/// <param name="selector">字段key selector</param>
		/// <param name="value">value</param>
		/// <returns></returns>
		private UpdateBuilder<TModel> Set<TKey>(Expression<Func<TModel, TKey>> selector, [DisallowNull]Enum value) where TKey : IFormattable
			=> Set(selector, (TKey)Convert.ChangeType(value, typeof(TKey)));

		/// <summary>
		/// 设置整型等于一个枚举
		/// </summary>
		/// <param name="selector">字段key selector</param>
		/// <param name="value">value</param>
		/// <returns></returns>
		private UpdateBuilder<TModel> Set<TKey>(Expression<Func<TModel, TKey?>> selector, Enum value) where TKey : struct, IFormattable
		{
			var field = SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText;
			if (value == null)
				return AddSetExpression(string.Format("{0} = null", field));

			AddParameterT((TKey)Convert.ChangeType(value, typeof(TKey)), out string valueIndex);
			return AddSetExpression(string.Format("{0} = @{1}", field, valueIndex));
		}

		/// <summary>
		/// 设置一个字段值(可空类型)
		/// </summary>
		/// <typeparam name="TKey">字段类型</typeparam>
		/// <param name="selector">字段key selector</param>
		/// <param name="value">value</param>
		/// <returns></returns>
		public UpdateBuilder<TModel> Set<TKey>(Expression<Func<TModel, TKey?>> selector, TKey? value) where TKey : struct
		{
			var field = SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText;
			if (value == null)
				return AddSetExpression(string.Format("{0} = null", field));
			AddParameterT(value.Value, out string valueIndex);
			return AddSetExpression(string.Format("{0} = @{1}", field, valueIndex));
		}

		/// <summary>
		/// 数组连接一个数组
		/// </summary>
		/// <typeparam name="TKey">数组类型</typeparam>
		/// <param name="selector">key selector</param>
		/// <param name="value">数组</param>
		/// <returns></returns>
		public UpdateBuilder<TModel> SetAppend<TKey>(Expression<Func<TModel, IEnumerable<TKey>>> selector, params TKey[] value)
		{
			AddParameterT(value, out string valueIndex);
			return AddSetExpression(string.Format("{0} = {0} || @{1}", SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText, valueIndex));
		}

		/// <summary>
		/// 数组移除某元素
		/// </summary>
		/// <typeparam name="TKey">数组的类型</typeparam>
		/// <param name="selector">key selector</param>
		/// <param name="value">元素</param>
		/// <returns></returns>
		public UpdateBuilder<TModel> SetRemove<TKey>(Expression<Func<TModel, IEnumerable<TKey>>> selector, TKey value)
		{
			AddParameterT(value, out string valueIndex);
			return AddSetExpression(string.Format("{0} = array_remove({0}, @{1})", SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText, valueIndex));
		}

		/// <summary>
		/// 自增, 可空类型留默认值
		/// </summary>
		/// <typeparam name="TKey">COALESCE默认值类型</typeparam>
		/// <typeparam name="TTarget">增加值的类型</typeparam>
		/// <param name="selector">key selector</param>
		/// <param name="value">增量</param>
		/// <param name="defaultValue">COALESCE默认值, 如果null, 则取default(TKey)</param>
		/// <exception cref="ArgumentNullException">增量为空</exception>
		/// <returns></returns>
		public UpdateBuilder<TModel> SetIncrement<TKey, TTarget>(Expression<Func<TModel, TKey?>> selector, TTarget value, TKey? defaultValue) where TKey : struct
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			var field = SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText;
			AddParameterT(value, out string valueIndex);
			AddParameterT(defaultValue ?? default, out string defaultValueIndex);
			return AddSetExpression(string.Format("{0} = COALESCE({0}, @{1}) + @{2}", field, defaultValueIndex, valueIndex));
		}

		/// <summary>
		/// 自增, 不可空类型不留默认值
		/// </summary>
		/// <typeparam name="TTarget">增加值的类型</typeparam>
		/// <typeparam name="TKey">原类型</typeparam>
		/// <param name="selector">key selector</param>
		/// <param name="value">增量</param>
		/// <exception cref="ArgumentNullException">增量为空</exception>
		/// <returns></returns>
		public UpdateBuilder<TModel> SetIncrement<TKey, TTarget>(Expression<Func<TModel, TKey>> selector, TTarget value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			var field = SqlExpressionVisitor.Instance.VisitSingleForNoAlias(selector).SqlText;
			AddParameterT(value, out string valueIndex);
			return AddSetExpression(string.Format("{0} = {0} + @{1}", field, valueIndex));
		}

		/// <summary>
		/// 返回修改行数
		/// </summary>
		/// <returns></returns>
		public new int ToRows() => base.ToRows();

		/// <summary>
		/// 返回修改行数, 并且ref实体类(一行)
		/// </summary>
		/// <returns></returns>
		public int ToRows(ref TModel refInfo)
		{
			_isReturn = true;
			var info = base.ToOne<TModel>();
			if (info == null) return 0;
			refInfo = info;
			return 1;
		}

		/// <summary>
		/// 返回修改行数, 并且ref列表(多行)
		/// </summary>
		/// <param name="refInfo"></param>
		/// <returns></returns>
		public int ToRows(ref List<TModel> refInfo)
		{
			_isReturn = true;
			var info = base.ToList<TModel>();
			refInfo = info;
			return info.Count;
		}

		/// <summary>
		/// 管道模式
		/// </summary>
		/// <returns></returns>
		public UpdateBuilder<TModel> ToRowsPipe() => base.ToPipe<int>(PipeReturnType.Rows);

		#region Override
		public override string ToString() => base.ToString();

		/// <summary>
		/// 获取sql语句
		/// </summary>
		/// <exception cref="ArgumentNullException">count of where or set is 0</exception>
		/// <returns></returns>
		public override string GetCommandTextString()
		{
			if (WhereList.Count == 0)
				throw new ArgumentNullException(nameof(WhereList));
			if (_setList.Count == 0)
				throw new ArgumentNullException(nameof(_setList));
			var ret = string.Empty;
			if (_isReturn)
			{
				Fields = EntityHelper.GetModelTypeFieldsString<TModel>(MainAlias);
				ret = $"RETURNING {Fields}";
			}
			return $"UPDATE {MainTable} {MainAlias} SET {string.Join(",", _setList)} WHERE {string.Join("\nAND", WhereList)} {ret}";
		}
		#endregion
	}
}
