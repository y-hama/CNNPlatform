using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Functions.Source
{
    class None : SourceCode
    {
        public override string Name
        {
            get { return @"None"; }
        }

        protected override void ParameterConfigration()
        {
            AddParameter("input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("output", ObjectType.Array, ElementType.FLOAT);

            AddParameter("len", ObjectType.Value, ElementType.INT);
            AddParameter("area", ObjectType.Value, ElementType.INT);
        }

        protected override void CreateSource()
        {
            GlobalID(1);
            AddMethodBody(@"
int min, max;
if (i0 - area > 0) { min = i0 - area; } else { min = 0; }
if (i0 + area < len) { max = i0 + area; } else { max = len - 1; }
output[i0] = 0;

for (int i = min; i < max; i++)
{
    output[i0] += (input[i]);
}
output[i0] /= (max - min);
            ");
        }
    }
}
