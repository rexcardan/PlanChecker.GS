using ESAPIX.Constraints;
using ESAPIX.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ESAPIX_WPF_Example.CustomConstraints
{
    public class MaxgEUDConstraint : IConstraint
    {
        private string _strId;
        private DoseValue _limit;
        private double _aValue;

        public MaxgEUDConstraint(string strId, DoseValue limit, double aValue)
        {
            _strId = strId;
            _limit = limit;
            _aValue = aValue;
        }

        public string Name => "gEUD Constraint";
        public string FullName => $"gEUD Constraint ({_strId}, Limit: {_limit})";

        public ConstraintResult CanConstrain(PlanningItem pi)
        {
            var ss = pi.StructureSet;
            if (ss != null && ss.Structures.Any(s => s.Id == _strId))
            {
                return new ConstraintResult(this, ResultType.PASSED, "Structure found");
            }
            return new ConstraintResult(this, ResultType.NOT_APPLICABLE, "Structure not found");
        }

        public ConstraintResult Constrain(PlanningItem pi)
        {
            DoseValuePresentation desiredDP = _limit.IsRelativeDoseValue ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute;
            var focusedStructure = pi.StructureSet.Structures.FirstOrDefault(s => s.Id == _strId);
            var dvh = pi.GetDVHCumulativeData(focusedStructure, desiredDP, VolumePresentation.Relative, 0.01);
            var difDvh = dvh.CurveData.Differential();

            // Calculate gEUD according to the formula: (Σ vᵢ Dᵢ^a)^(1/a)
            var sum = difDvh.Sum(d => d.Volume * Math.Pow(d.DoseValue.GetDose(_limit.Unit), _aValue));
            var gEUD = Math.Pow(sum, 1.0 / _aValue);

            // Check if gEUD exceeds the limit
            if (gEUD > _limit.Dose)
            {
                return new ConstraintResult(this, ResultType.ACTION_LEVEL_2, $"gEUD: {gEUD:N2} exceeds limit: {_limit}");
            }
            else
            {
                return new ConstraintResult(this, ResultType.PASSED, $"gEUD: {gEUD:N2} is within limit: {_limit}");
            }
        }
    }
}
