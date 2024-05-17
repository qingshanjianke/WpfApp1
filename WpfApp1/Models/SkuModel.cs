using SqlSugar;

namespace WpfApp1.Models
{
    public class SkuModel
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string skuCode { get; set; }


        [SugarColumn(IsNullable = true)]
        public string skuName { get; set; }

        [SugarColumn(IsNullable = true)]
        public string skuItemNo { get; set; }

        /// <summary>
        /// 单价（分）
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int price { get; set; }

        [SugarColumn(IsNullable = true)]
        public byte priceUnit { get; set; }

        [SugarColumn(IsNullable = true)]
        public string unitName { get; set; }

        [SugarColumn(IsNullable = true)]
        public int status { get; set; }

        [SugarColumn(IsNullable = true)]
        public string iconName { get; set; }

        [SugarColumn(IsNullable = true)]
        public string iconPath { get; set; }

        [SugarColumn(IsNullable = true)]
        public string iconUrl { get; set; }

        [SugarColumn(IsNullable = true)]
        public string pinyin { get; set; }

        [SugarColumn(IsNullable = true)]
        public int featNum { get; set; }

        [SugarColumn(IsNullable = true)]
        public long seqNo { get; set; }

        [SugarColumn(IsNullable = true)]
        public byte isDeleted { get; set; }

        [SugarColumn(IsNullable = true)]
        public long createTime { get; set; }

        [SugarColumn(IsNullable = true)]
        public long updateTime { get; set; }
    }
}
