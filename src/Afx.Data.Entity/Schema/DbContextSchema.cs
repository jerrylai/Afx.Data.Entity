using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.Reflection;
#if NETCOREAPP || NETSTANDARD
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace Afx.Data.Entity.Schema
{
    /// <summary>
    /// 生成 DbContext
    /// </summary>
    public class DbContextSchema : IDbContextSchema
    {
        /// <summary>
        /// 获取 DbContext .cs 文件的代码
        /// </summary>
        /// <param name="contextName">DbContext名称</param>
        /// <param name="models">生成dbset的model 名称</param>
        /// <param name="namespace">DbContext .cs 文件命名空间</param>
        /// <returns>.cs 文件源代码</returns>
        public virtual string GetContextCode(string contextName, List<string> models, string @namespace)
        {
            if (string.IsNullOrEmpty(contextName)) throw new ArgumentNullException("contextName");
            if (models == null) throw new ArgumentNullException("models");
            if (string.IsNullOrEmpty(@namespace)) throw new ArgumentNullException("namespace");
            StringBuilder modelsString = new StringBuilder();
            if (models.Count > 0)
            {
                foreach (var m in models)
                {
                    modelsString.Append(this.GetPropertyCode(m));
                    modelsString.Append("\r\n\r\n");
                }
                modelsString.Remove(modelsString.Length - 2, 2);
            }

            return string.Format(this.GetDbContextFormat(), @namespace, contextName, modelsString.ToString());
        }

        /// <summary>
        /// 获取 DbContext DbSet 代码
        /// </summary>
        /// <param name="model">DbSet model名称</param>
        /// <returns>DbContext DbSet 源代码</returns>
        public virtual string GetPropertyCode(string model)
        {
            if (string.IsNullOrEmpty(model)) throw new ArgumentNullException("model");
            return string.Format(this.GetPropertyFormat(), model);
        }

        /// <summary>
        /// 获取DbContext名称
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <returns>DbContext名称</returns>
        public virtual string GetContextName(string database)
        {
            if (string.IsNullOrEmpty(database)) throw new ArgumentNullException("database");
            string name = database;

            name.Replace('.', '_');
            char c = name[0];
            if ('0' <= c && c <= '9' || c == '#' || c == '$')
            {
                name = "_" + name;
            }

            return name;
        }
       
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            dbContextFormat = null;
            dbContextPropertyFormat = null;
        }

        private string dbContextFormat;
        /// <summary>
        /// GetDbContextFormat
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDbContextFormat()
        {
            if (string.IsNullOrEmpty(this.dbContextFormat))
            {
                try
                {
                    string file = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), DbContextFormatFile);
                    string path = System.IO.Path.GetDirectoryName(file);
                    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                    if (!System.IO.File.Exists(file))
                    {
                        System.IO.File.WriteAllText(file, DbContextFormat, Encoding.UTF8);
                    }
                    else
                    {
                        this.dbContextFormat = System.IO.File.ReadAllText(file, Encoding.UTF8);
                    }
                }
                catch { }
                if (string.IsNullOrEmpty(this.dbContextFormat)) this.dbContextFormat = DbContextFormat;
            }

            return this.dbContextFormat;
        }

        private string dbContextPropertyFormat;
        /// <summary>
        /// GetPropertyFormat
        /// </summary>
        /// <returns></returns>
        protected virtual string GetPropertyFormat()
        {
            if (string.IsNullOrEmpty(this.dbContextPropertyFormat))
            {
                try
                {
                    string file = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), DbContextPropertyFormatFile);
                    string path = System.IO.Path.GetDirectoryName(file);
                    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                    if (!System.IO.File.Exists(file))
                    {
                        System.IO.File.WriteAllText(file, DbContextPropertyFormat, Encoding.UTF8);
                    }
                    else
                    {
                        this.dbContextPropertyFormat = System.IO.File.ReadAllText(file, Encoding.UTF8);
                    }
                }
                catch { }
                if (string.IsNullOrEmpty(this.dbContextPropertyFormat)) this.dbContextPropertyFormat = DbContextPropertyFormat;
            }

            return this.dbContextPropertyFormat;
        }

        private const string DbContextPropertyFormatFile = "template\\DbContextProperty.tmpl";
        private const string DbContextPropertyFormat = @"        public DbSet<{0}> {0} {{ get; set; }}";
#if NETCOREAPP || NETSTANDARD
        private const string DbContextFormatFile = "template\\DbContextCore.tmpl";

        private const string DbContextFormat = @"using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

using Afx.Data.Entity;

namespace {0}
{{
    public partial class {1}Context : EntityContext
    {{
        public {1}Context()
            : base(new DbContextOptions<{1}Context>())
        {{
            
        }}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
             //optionsBuilder.UseSqlServer(ConfigUtils.ConnectionString);
            //optionsBuilder.UseMySQL(ConfigUtils.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        #region Models
{2}
        #endregion
    }}
}}";
#else
        private const string DbContextFormatFile = "template\\DbContext.tmpl";

        private const string DbContextFormat = @"using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.Entity;

using Afx.Data.Entity;

namespace {0}
{{
    public partial class {1}Context : EntityContext
    {{
        public {1}Context(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {{
            
        }}

        public {1}Context(DbConnection dbConnection)
            : base(dbConnection)
        {{
            
        }}

        #region Models
{2}
        #endregion
    }}
}}";
#endif

    }
}
