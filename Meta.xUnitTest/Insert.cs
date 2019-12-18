using Meta.Common.Interface;
using Meta.Common.SqlBuilder;
using Meta.xUnitTest.DAL;
using Meta.xUnitTest.Model;
using Meta.xUnitTest.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Meta.xUnitTest
{
    [Order(1)]
    public class Insert : BaseTest
    {
        [Fact, Order(1), Description("name of create_time can be ignored if use 'Create_time = Datetime.Now' in ModelInsert")]
        public void ModelInsertReturnModel()
        {
            var info = People.GetItem(StuPeopleId1);
            if (info == null)
            {
                info = new PeopleModel
                {
                    Address = "xxx",
                    Id = StuPeopleId1,
                    Age = 10,
                    Create_time = DateTime.Now, // you can ignore if use Datetime.Now;
                    Name = "leisaupei",
                    Sex = true,
                    State = EDataState.正常,

                }.Insert();

                Assert.NotNull(info);
            }
            info = People.GetItem(StuPeopleId2);
            if (info == null)
            {
                // else you can
                info = People.Insert(new PeopleModel
                {
                    Address = "xxx",
                    Id = StuPeopleId2,
                    Age = 10,
                    Create_time = DateTime.Now,
                    Name = "leisaupei",
                    Sex = true,
                    State = EDataState.正常
                });
                Assert.NotNull(info);
            }
        }
        [Fact, Order(2)]
        public void ModelInsertReturnModifyRows()
        {
            var info = People.GetItem(StuPeopleId2);
            if (info != null) return;

            var row = new PeopleModel
            {
                Address = "xxx",
                Id = StuPeopleId2,
                Age = 10,
                Create_time = DateTime.Now,
                Name = "nickname",
                Sex = true,
                State = EDataState.正常,

            }.Commit();
            //else you can
            //row = Teacher.Commit(new TeacherModel
            //{
            //	Address = "xxx",
            //	Id = StuPeopleId2,
            //	Age = 10,
            //	Create_time = DateTime.Now,
            //	Name = "nickname",
            //	Sex = true

            //});
            Assert.Equal(1, row);
        }
        [Fact, Order(3)]
        public void InsertCustomizedDictonary()
        {
            var info = ClassGrade.GetItem(GradeId);
            if (info != null) return;

            var affrows = ClassGrade.InsertDiy.Set(a => a.Id, GradeId)
                .Set(f => f.Name, "移动互联网")
                .Set(a => a.Create_time, DateTime.Now)
                .ToRows(ref info); //return modify rows ref model
            Assert.NotNull(info);
            Assert.Equal(1, affrows);
        }
        [Fact, Order(4)]
        public void InsertCustomized()
        {
            var info = Student.GetItemByPeople_id(StuPeopleId1);
            if (info != null) return;
            var affrows = Student.InsertDiy.Set(f => f.Id, Guid.NewGuid())
                            .Set(a => a.People_id, StuPeopleId1)
                            .Set(a => a.Stu_no, StuNo1)
                            .Set(a => a.Grade_id, GradeId)
                            .Set(a => a.Create_time, DateTime.Now)
                            .ToRows(ref info);
            Assert.NotNull(info);
            Assert.Equal(1, affrows);

            var info1 = Student.GetItemByPeople_id(StuPeopleId2);
            if (info1 != null) return;
            var affrows1 = Student.InsertDiy.Set(a => a.Id, Guid.NewGuid())
                            .Set(a => a.People_id, StuPeopleId2)
                            .Set(a => a.Stu_no, StuNo2)
                            .Set(a => a.Grade_id, GradeId)
                            .Set(a => a.Create_time, DateTime.Now)
                            .ToRows(ref info1);
            Assert.NotNull(info1);
            Assert.Equal(1, affrows1);
        }

    }
}