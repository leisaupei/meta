﻿using Meta.Common.Model;
using Meta.Common.SqlBuilder.AnalysisExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Meta.Common.SqlBuilder
{
	public abstract class SelectBuilder<TSQL> : WhereBase<TSQL> where TSQL : class, new()
	{
		readonly List<UnionModel> _listUnion = new List<UnionModel>();
		string _groupBy;
		string _orderBy;
		string _limit;
		string _offset;
		string _having;
		string _union;
		string _tablesampleSystem;
		#region Constructor
		protected SelectBuilder(string fields, string alias)
		{
			Fields = fields;
			MainAlias = alias;
		}
		public SelectBuilder(string fields) => Fields = fields;
		public SelectBuilder() => Fields = "*";
		#endregion

		TSQL This => this as TSQL;
		/// <summary>
		/// from 表名 别名
		/// </summary>
		/// <param name="table"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public TSQL From(string table, string alias = "a")
		{
			MainAlias = alias;
			if (new Regex(@"^SELECT\s.+\sFROM\s").IsMatch(table))
				MainTable = $"({table})";
			else
				MainTable = table;
			return This;
		}
		/// <summary>
		/// sql语句group by
		/// </summary>
		/// <param name="s"></param>
		/// <example>GroupBy("xxx,xxx")</example>
		/// <returns></returns>
		public TSQL GroupBy(string s)
		{
			if (!string.IsNullOrEmpty(_groupBy))
				_groupBy += ", ";
			_groupBy = s;
			return This;
		}
		/// <summary>
		/// sql语句order by
		/// </summary>
		/// <param name="s"></param>
		/// <example>OrderBy("xxx desc,xxx asc")</example>
		/// <returns></returns>
		public TSQL OrderBy(string s)
		{
			if (!string.IsNullOrEmpty(_orderBy))
				_orderBy += ", ";
			_orderBy = s;
			return This;
		}
		/// <summary>
		/// having
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public TSQL Having(string s)
		{
			_having = s;
			return This;
		}
		/// <summary>
		/// limit
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public TSQL Limit(int i)
		{
			_limit = i.ToString();
			return This;
		}
		/// <summary>
		/// 等于数据库offset
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public TSQL Skip(int i)
		{
			_offset = i.ToString();
			return This;
		}
		/// <summary>
		/// 连接一个sql语句
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public TSQL Union(string view)
		{
			_union = $"({view})";
			return This;
		}
		/// <summary>
		/// 连接 selectbuilder
		/// </summary>
		/// <param name="selectBuilder"></param>
		/// <returns></returns>
		public TSQL Union(TSQL selectBuilder)
		{
			_union = $"({selectBuilder})";
			return This;
		}
		/// <summary>
		/// 分页
		/// </summary>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public TSQL Page(int pageIndex, int pageSize)
		{
			Limit(pageSize); Skip(Math.Max(0, pageIndex - 1) * pageSize);
			return This;
		}
		/// <summary>
		/// 随机抽样
		/// </summary>
		/// <param name="percent">seed</param>
		/// <returns></returns>
		public TSQL TableSampleSystem(double percent)
		{
			_tablesampleSystem = $" tablesample system({percent}) ";
			return This;
		}
		public TSQL OrderBy<TModel, TResult>(Expression<Func<TModel, TResult>> selector)
		{
			var ter = new ExpressionTerminator(selector);
			OrderBy(string.Concat(ter.GetResult()));
			return This;
		}
		public TSQL GroupBy<TModel, TResult>(Expression<Func<TModel, TResult>> selector)
		{
			var ter = new ExpressionTerminator(selector);
			GroupBy(string.Concat(ter.GetResult()));
			return This;
		}
		public TSQL OrderByDescing<TModel, TResult>(Expression<Func<TModel, TResult>> selector)
		{
			var ter = new ExpressionTerminator(selector);
			OrderBy(string.Concat(ter.GetResult(), " desc"));
			return This;
		}
		#region Union
		public TSQL InnerJoin<TDal>(SelectBuilder<TDal> selectBuilder, string alias, string on) where TDal : class, new()
			=> Join(UnionEnum.INNER_JOIN, $"({selectBuilder})", alias, on);
		public TSQL LeftJoin<TDal>(SelectBuilder<TDal> selectBuilder, string alias, string on) where TDal : class, new()
			=> Join(UnionEnum.LEFT_JOIN, $"({selectBuilder})", alias, on);
		public TSQL RightJoin<TDal>(SelectBuilder<TDal> selectBuilder, string alias, string on) where TDal : class, new()
			=> Join(UnionEnum.RIGHT_JOIN, $"({selectBuilder})", alias, on);
		//public TSQL InnerJoin<T, TTarget>(Expression<Func<T, TTarget, bool>> predicate)
		//{
		//	return This;
		//}
		public TSQL InnerJoin(string table, string alias, string on) => Join(UnionEnum.INNER_JOIN, table, alias, on);
		public TSQL LeftJoin(string table, string alias, string on) => Join(UnionEnum.LEFT_JOIN, table, alias, on);
		public TSQL RightJoin(string table, string alias, string on) => Join(UnionEnum.RIGHT_JOIN, table, alias, on);
		public TSQL InnerJoin<TDal>(string alias, string on, bool isReturn = false) where TDal : SelectBuilder<TDal>, new() => Join<TDal>(UnionEnum.INNER_JOIN, alias, on, isReturn);
		public TSQL LeftJoin<TDal>(string alias, string on, bool isReturn = false) where TDal : SelectBuilder<TDal>, new() => Join<TDal>(UnionEnum.LEFT_JOIN, alias, on, isReturn);
		public TSQL RightJoin<TDal>(string alias, string on, bool isReturn = false) where TDal : SelectBuilder<TDal>, new() => Join<TDal>(UnionEnum.RIGHT_JOIN, alias, on, isReturn);
		public TSQL InnerJoinRef<TDal>(string alias, string on) where TDal : SelectBuilder<TDal>, new() => Join<TDal>(UnionEnum.INNER_JOIN, alias, on, true);
		public TSQL LeftJoinRef<TDal>(string alias, string on) where TDal : SelectBuilder<TDal>, new() => Join<TDal>(UnionEnum.LEFT_JOIN, alias, on, true);
		public TSQL RightJoinRef<TDal>(string alias, string on) where TDal : SelectBuilder<TDal>, new() => Join<TDal>(UnionEnum.RIGHT_JOIN, alias, on, true);
		public TSQL Join<TDal>(UnionEnum unionType, string alias, string on, bool isReturn = false)
		{
			_listUnion.Add(UnionModel.Create<TDal>(alias, on, unionType, isReturn));
			return This;
		}

		public TSQL Join(UnionEnum unionType, string table, string aliasName, string on)
		{
			_listUnion.Add(new UnionModel(aliasName, table, on, unionType));
			return This;
		}
		#endregion
		/// <summary>
		/// 返回一行(管道)
		/// </summary>
		public TSQL ToListPipe<T>(string fields = null)
		{
			if (!string.IsNullOrEmpty(fields)) Fields = fields;
			return base.ToPipe<T>(PipeReturnType.List);
		}
		/// <summary>
		/// 返回列表
		/// </summary>
		public List<T> ToList<T>(string fields = null)
		{
			if (!string.IsNullOrEmpty(fields)) Fields = fields;
			if (IsReturnDefault) return new List<T>();

			return base.ToList<T>();
		}
		/// <summary>
		/// 返回一行(管道)
		/// </summary>
		public TSQL ToOnePipe<T>(string fields = null)
		{
			_limit = "1";
			if (!string.IsNullOrEmpty(fields)) Fields = fields;
			return base.ToPipe<T>(PipeReturnType.One);
		}
		/// <summary>
		/// 返回一行
		/// </summary>
		public T ToOne<T>(string fields = null)
		{
			_limit = "1";
			if (!string.IsNullOrEmpty(fields)) Fields = fields;
			return base.ToOne<T>();
		}
		/// <summary>
		/// 返回第一个元素
		/// </summary>
		public TResult ToScalar<TResult>(string fields)
		{
			Fields = fields;
			return (TResult)ToScalar();
		}

		public long Count() => ToScalar<long>("COUNT(1)");
		public TResult Max<TResult>(string field, string defaultValue = "0") => ToScalar<TResult>($"COALESCE(MAX({field}),{defaultValue})");
		public TResult Min<TResult>(string field, string defaultValue = "0") => ToScalar<TResult>($"COALESCE(MIN({field}),{defaultValue})");
		public TResult Sum<TResult>(string field, string defaultValue = "0") => ToScalar<TResult>($"COALESCE(SUM({field}),{defaultValue})");
		public TResult Avg<TResult>(string field, string defaultValue = "0") => ToScalar<TResult>($"COALESCE(AVG({field}),{defaultValue})");



		#region Override
		public override string ToString() => base.ToString();
		public new string ToString(string field) => base.ToString(field);
		public override string GetCommandTextString()
		{
			var field = new StringBuilder(Fields);
			var union = new StringBuilder();
			foreach (var item in _listUnion)
			{
				union.AppendLine(string.Format("{0} {1} {2} ON {3}", item.UnionTypeString, item.Table, item.AliasName, item.Expression));
				if (item.IsReturn)
					field.Append(", ").Append(item.Fields);
			}
			StringBuilder sqlText = new StringBuilder($"SELECT {field} FROM {MainTable} {MainAlias} {_tablesampleSystem} {union}");

			// other
			if (WhereList?.Count() > 0)
				sqlText.AppendLine("WHERE " + string.Join(" AND ", WhereList));

			if (!string.IsNullOrEmpty(_groupBy))
				sqlText.AppendLine(string.Concat("GROUP BY ", _groupBy));

			if (!string.IsNullOrEmpty(_groupBy) && !string.IsNullOrEmpty(_having))
				sqlText.AppendLine(string.Concat("HAVING ", _having));

			if (!string.IsNullOrEmpty(_orderBy))
				sqlText.AppendLine(string.Concat("ORDER BY ", _orderBy));

			if (!string.IsNullOrEmpty(_limit))
				sqlText.AppendLine(string.Concat("LIMIT ", _limit));

			if (!string.IsNullOrEmpty(_offset))
				sqlText.AppendLine(string.Concat("OFFSET ", _offset));

			if (!string.IsNullOrEmpty(_union))
				sqlText.AppendLine(string.Concat("UNION ", _union));
			return sqlText.ToString();
		}
		#endregion
	}
}
