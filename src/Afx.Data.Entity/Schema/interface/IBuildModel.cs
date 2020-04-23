using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 根据数据库结构生成 EntityContext 与 model .cs 文件
    /// </summary>
    public interface IBuildModel : IDisposable
    {
        /// <summary>
        /// 生成 model .cs 文件
        /// </summary>
        /// <param name="_namespase">.cs 文件命名空间</param>
        /// <param name="dir">存放目录</param>
        void Build(string _namespase, string dir);
    }
}
