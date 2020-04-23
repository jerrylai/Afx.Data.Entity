using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.Entity.Schema
{
    public class TableInfoModel : Object
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string Comment { get; set; }
    }
}
