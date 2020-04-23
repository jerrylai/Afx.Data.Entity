using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 更新数据库结构类型
    /// </summary>
    public interface IBuildDatabase : IDisposable
    {
        /// <summary>
        /// 创建、更新数据库结构，添加 更新、删除索引， 列只增加，不修改，不删除
        /// </summary>
        /// <typeparam name="T">EntityContext</typeparam>
        void Build<T>() where T : EntityContext;
    }
}
