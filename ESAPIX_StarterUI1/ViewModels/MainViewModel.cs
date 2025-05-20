using ESAPIX.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESAPIX.Common;
using VMS.TPS.Common.Model.API;
using Prism.Commands;
using System.Windows;
using ESAPIX.Extensions;
using ESAPIX.Constraints.DVH;
using System.Collections.ObjectModel;
using ESAPIX_WPF_Example.CustomConstraints;
using VMS.TPS.Common.Model.Types;

namespace ESAPX_StarterUI.ViewModels
{
    public class MainViewModel : BindableBase
    {
        AppComThread VMS = AppComThread.Instance;

        public MainViewModel()
        {
            EvaluateCommand = new DelegateCommand(() =>
            {
                foreach (var pc in Constraints)
                {
                    var result = VMS.GetValue(sc =>
                    {
                        //Check if we can constrain first
                        var canConstrain = pc.Constraint.CanConstrain(sc.PlanSetup);
                        //If not..report why
                        if (!canConstrain.IsSuccess) { return canConstrain; }
                        else
                        {
                            //Can constrain - so do it
                            return pc.Constraint.Constrain(sc.PlanSetup);
                        }
                    });
                    //Update UI
                    pc.Result = result;
                }
            });

            CreateConstraints();
        }

        private void CreateConstraints()
        {
            Constraints.AddRange(new ConstraintResultPair[]
            {
                new ConstraintResultPair(ConstraintBuilder.Build("PTV45", "Max[%] <= 110")),
                new ConstraintResultPair(ConstraintBuilder.Build("Rectum", "V75Gy[cc] <= 15")),
                new ConstraintResultPair(ConstraintBuilder.Build("Rectum", "V65Gy[%] <= 35")),
                new ConstraintResultPair(ConstraintBuilder.Build("Bladder", "V80Gy[%] <= 15")),
                new ConstraintResultPair(new MaxgEUDConstraint("Rectum", new DoseValue(60, DoseValue.DoseUnit.Gy), 40)),
              //  new PlanConstraint(new CTDateConstraint())
            });
        }


        public DelegateCommand EvaluateCommand { get; set; }
        public ObservableCollection<ConstraintResultPair> Constraints { get; private set; } = new ObservableCollection<ConstraintResultPair>();
    }
}