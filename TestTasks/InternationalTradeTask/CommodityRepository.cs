using System;
using System.Collections.Generic;
using System.Linq;
using TestTasks.InternationalTradeTask.Models;

namespace TestTasks.InternationalTradeTask
{
    public class CommodityRepository
    {
        public double GetImportTariff(string commodityName)
        {
            var commodity = FindCommodityGroup(commodityName);
            if (commodity == null)
                throw new ArgumentException($"Commodity '{commodityName}' not found.");

            return GetEffectiveImportTariff(commodity);
        }
        private double GetEffectiveImportTariff(ICommodityGroup commodity)
        {
            while (commodity != null)
            {
                if (commodity.ImportTarif.HasValue)
                    return commodity.ImportTarif.Value;

                commodity = GetParentCommodityGroup(commodity);
            }
            return 0;
        }
        private ICommodityGroup GetParentCommodityGroup(ICommodityGroup commodity)
        {
            foreach (var group in _allCommodityGroups)
            {
                var parent = FindParentRecursive(group, commodity);
                if (parent != null)
                    return parent;
            }
            return null;
        }
        private ICommodityGroup FindParentRecursive(ICommodityGroup parent, ICommodityGroup child)
        {
            if (parent.SubGroups != null && parent.SubGroups.Contains(child))
                return parent;

            if (parent.SubGroups != null)
            {
                foreach (var subGroup in parent.SubGroups)
                {
                    var result = FindParentRecursive(subGroup, child);
                    if (result != null) return result;
                }
            }
            return null;
        }
        public double GetExportTariff(string commodityName)
        {
            var commodity = FindCommodityGroup(commodityName);
            if (commodity == null)
                throw new ArgumentException($"Commodity '{commodityName}' not found.");

            return GetEffectiveExportTariff(commodity);
        }
        private double GetEffectiveExportTariff(ICommodityGroup commodity)
        {
            while (commodity != null)
            {
                if (commodity.ExportTarif.HasValue)
                    return commodity.ExportTarif.Value;

                commodity = GetParentCommodityGroup(commodity);
            }
            return 0;
        }
        private ICommodityGroup FindCommodityGroup(string commodityName)
        {
            foreach (var group in _allCommodityGroups)
            {
                var result = FindCommodityGroupRecursive(group, commodityName);
                if (result != null) return result;
            }
            return null;
        }
        private ICommodityGroup FindCommodityGroupRecursive(ICommodityGroup group, string commodityName)
        {
            if (group.Name.Equals(commodityName, StringComparison.OrdinalIgnoreCase))
                return group;

            if (group.SubGroups != null)
            {
                foreach (var subGroup in group.SubGroups)
                {
                    var result = FindCommodityGroupRecursive(subGroup, commodityName);
                    if (result != null) return result;
                }
            }
            return null;
        }

        private FullySpecifiedCommodityGroup[] _allCommodityGroups = new FullySpecifiedCommodityGroup[]
        {
            new FullySpecifiedCommodityGroup("06", "Sugar, sugar preparations and honey", 0.05, 0)
            {
                SubGroups = new CommodityGroup[]
                {
                    new CommodityGroup("061", "Sugar and honey")
                    {
                        SubGroups = new CommodityGroup[]
                        {
                            new CommodityGroup("0611", "Raw sugar,beet & cane"),
                            new CommodityGroup("0612", "Refined sugar & other prod.of refining,no syrup"),
                            new CommodityGroup("0615", "Molasses", 0, 0),
                            new CommodityGroup("0616", "Natural honey", 0, 0),
                            new CommodityGroup("0619", "Sugars & syrups nes incl.art.honey & caramel"),
                        }
                    },
                    new CommodityGroup("062", "Sugar confy, sugar preps. Ex chocolate confy", 0, 0)
                }
            },
            new FullySpecifiedCommodityGroup("282", "Iron and steel scrap", 0, 0.1)
            {
                SubGroups = new CommodityGroup[]
                {
                    new CommodityGroup("28201", "Iron/steel scrap not sorted or graded"),
                    new CommodityGroup("28202", "Iron/steel scrap sorted or graded/cast iron"),
                    new CommodityGroup("28203", "Iron/steel scrap sort.or graded/tinned iron"),
                    new CommodityGroup("28204", "Rest of 282.0")
                }
            }
        };
    }
}
