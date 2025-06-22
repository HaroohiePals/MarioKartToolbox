using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.MapObj.Enemies;

class TrafficLogicPart : LogicPart<Traffic>
{
    public TrafficLogicPart(MkdsContext context) 
        : base(context, LogicPartType.Type0)
    {
    }

    protected override void Update(Traffic instance)
        => instance.Update();
}
