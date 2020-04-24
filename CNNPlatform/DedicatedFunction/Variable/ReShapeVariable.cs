using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.Locker;

namespace CNNPlatform.DedicatedFunction.Variable
{
    class ReshapeVariable : VariableBase
    {
        public override string GetStatus
        {
            get
            {
                string ext = string.Empty;
                return ext;
            }
        }

        public override void CoreClone(ref VariableBase _clone)
        {
        }

        public override string EncodeOption()
        {
            string res = string.Empty;
            return res;
        }

        public override void OverwriteParameter(ref object parameter)
        {
        }

        public override void SaveObject(DirectoryInfo location)
        {
        }

        public override void UpdateParameter(object parameter)
        {
        }

        protected override void ConfirmField(object shared)
        {
        }

        protected override void DecodeOption(List<object> values)
        {
        }

        protected override void DecodeParameterCore(object[] values)
        {
        }

        protected override void EncodeParameterCore(ref string res)
        {
        }

        protected override void EncodeParameterCore(ref TagFileController.TagSegment container)
        {
        }
    }
}
