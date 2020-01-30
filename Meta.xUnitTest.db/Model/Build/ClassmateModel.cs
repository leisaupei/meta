﻿using Meta.Common.Model;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.NetworkInformation;
using NpgsqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Meta.Common.Interface;
using System.Xml;
using System.Net;
using Meta.Common.SqlBuilder;
using Meta.xUnitTest.DAL;

namespace Meta.xUnitTest.Model
{
	[DbTable("classmate")]
	public partial class ClassmateModel : IDbModel
	{
		#region Properties
		[JsonProperty] public Guid Teacher_id { get; set; }
		[JsonProperty] public Guid Student_id { get; set; }
		[JsonProperty] public Guid Grade_id { get; set; }
		[JsonProperty] public DateTime? Create_time { get; set; }
		#endregion

		#region Foreign Key
		private ClassGradeModel _getClassGrade = null;
		public ClassGradeModel GetClassGrade => _getClassGrade ??= ClassGrade.GetItem(Grade_id);

		private TeacherModel _getTeacher = null;
		public TeacherModel GetTeacher => _getTeacher ??= Teacher.GetItem(Teacher_id);
		#endregion

		#region Update/Insert
		public UpdateBuilder<ClassmateModel> Update => DAL.Classmate.Update(this);

		public int Delete() => DAL.Classmate.Delete(this);
		public int Commit() => DAL.Classmate.Commit(this);
		public ClassmateModel Insert() => DAL.Classmate.Insert(this);
		#endregion
	}
}
