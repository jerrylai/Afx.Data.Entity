using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 生成 DbContext
    /// </summary>
    public interface IDbContextSchema : IDisposable
    {
        /// <summary>
        /// 获取 DbContext .cs 文件的代码
        /// </summary>
        /// <param name="contextName">DbContext名称</param>
        /// <param name="models">生成dbset的model 名称</param>
        /// <param name="namespace">DbContext .cs 文件命名空间</param>
        /// <returns>.cs 文件源代码</returns>
        string GetContextCode(string contextName, List<string> models, string @namespace);

        /// <summary>
        /// 获取 DbContext DbSet 代码
        /// </summary>
        /// <param name="model">DbSet model名称</param>
        /// <returns>DbContext DbSet 源代码</returns>
        string GetPropertyCode(string model);

        /// <summary>
        /// 获取DbContext名称
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <returns>DbContext名称</returns>
        string GetContextName(string database);
    }
}
